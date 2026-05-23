using CondoNet.Accounting.Api.Endpoints;
using CondoNet.Accounting.Core.Repositories;
using CondoNet.Accounting.Core.Services;
using CondoNet.Accounting.Infrastructure.Consumers;
using CondoNet.Accounting.Infrastructure.Persistence;
using CondoNet.Accounting.Infrastructure.Repositories;
using CondoNet.Accounting.Infrastructure.Services;
using CondoNet.Shared.Interfaces;
using CondoNet.Shared.Middleware;
using CondoNet.Shared.Services;
using CondoNet.Shared.Settings;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando el microservicio Accounting Service de CondoNET...");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog antes de builder.Build()
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Evita spam de logs internos de .NET
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "AuthService") // Identifica que este log es de Auth
        .WriteTo.Console()
        .WriteTo.File("Logs/log-.txt",
            rollingInterval: RollingInterval.Day, // Un archivo por día: log-20240321.txt
            retainedFileCountLimit: 7,            // Borra logs viejos automáticamente (guarda 1 semana)
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ")); // Asegúrate que coincida con tu .env/appsettings

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
        ?? throw new InvalidOperationException("JwtSettings no configurado.");

    var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>()
        ?? throw new InvalidOperationException("RabbitMQ no configurado.");


    builder.Services.AddDbContext<AccountingDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CondoNet.Accounting.Infrastructure")));

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<ITenantService, TenantService>();
    // Repositorios
    builder.Services.AddScoped<IAccountingEntryRepository, AccountingEntryRepository>();
    builder.Services.AddScoped<IAccountingTransactionRepository, AccountingTransactionRepository>();
    builder.Services.AddScoped<IAccountRepository, AccountRepository>();


    // Servicios
    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<IAccountingAutomatonService, AccountingAutomatonService>();

    // 4. OpenAPI / Swagger
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(s =>
    {
        s.SwaggerDoc("v1", new OpenApiInfo { Title = "CondoNet Accounting API", Version = "v1" });

        // Configuración de Seguridad en Swagger
        s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Escribe el token JWT directamente."
        });

        s.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Name = "X-Api-Key",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "Ingresa tu API Key en el header X-Api-Key"
        });

        // Requiere ambos esquemas para todos los endpoints
        s.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = [],
            [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
        });

    });

    // 5. MassTransit con RabbitMQ (Simplificado)
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<InvoicePaidEventConsumer>();
        x.AddConsumer<InvoiceRegisteredEventConsumer>();
        x.AddConsumer<CondoCreatedEventConsumer>();
        x.UsingRabbitMq((context, cfg) =>
        {
            // Usamos solo el nombre del host (localhost o rabbitmq)
            cfg.Host(rabbitMqSettings.Host, (ushort)rabbitMqSettings.Port, "/", h =>
            {
                // Configuramos el puerto por separado
                h.Username(rabbitMqSettings.Username);
                h.Password(rabbitMqSettings.Password);
            });
        });
    });

    // 6. Autenticación JWT
    var key = Encoding.ASCII.GetBytes(jwtSettings.Secret); // Usamos .Secret de tu clase

    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdminRole", policy =>
            policy.RequireRole("ADMIN"))
        .AddPolicy("RequireAccountingRole", p => p.RequireRole("ADMIN", "AccountingManager"))
        .AddPolicy("RequiredAnyRole", p => p.RequireRole("ADMIN", "Manager", "User"));

    builder.Services.AddHttpClient("AuthService")
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            return handler;
        });

    var app = builder.Build();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "PostgreSQL");


    // 7. Pipeline de Middleware
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "CondoNet Accounting API V1");
        c.RoutePrefix = "swagger";
    });

    // Health check endpoint
    app.MapHealthChecks("/health");

    // Dentro de Program.cs antes de app.Run()
    app.Use(async (context, next) =>
    {
        var condoId = context.User.FindFirst("condo_id")?.Value ?? "N/A";
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        using (Serilog.Context.LogContext.PushProperty("CondoId", condoId))
        using (Serilog.Context.LogContext.PushProperty("UserId", userId))
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            // 3. Añadirlo a la respuesta para que el cliente pueda reportarlo en caso de error
            context.Response.Headers.Append("X-Correlation-ID", correlationId);

            await next();
        }
    });

    // app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // El middleware de ApiKey debe ir después de Auth si depende de claims, 
    // o antes si es independiente. Aquí lo dejamos antes del ruteo.
    app.UseMiddleware<ApiKeyMiddleware>();

    // 4. Mapeo de Minimal APIs
    app.MapAccountingEndpoints();
    app.MapAccountEndpoints();
    app.MapAccountingTransactionEndpoints();
    app.MapAccountingEntryEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Accounting service falló en el arranque.");
}
finally
{
    Log.CloseAndFlush();
}
