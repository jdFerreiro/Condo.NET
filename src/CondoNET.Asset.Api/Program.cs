using CondoNet.Asset.Api.Endpoints;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Asset.Infrastructure.Services;
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
    Log.Information("Iniciando el microservicio Asset Service de HabitApp...");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog antes de builder.Build()
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Evita spam de logs internos de .NET
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "AssetService") // Ajustado: Identifica que este log es de Asset
        .WriteTo.Console()
        .WriteTo.File("Logs/log-.txt",
            rollingInterval: RollingInterval.Day, // Un archivo por día: log-20240321.txt
            retainedFileCountLimit: 7,            // Borra logs viejos automáticamente (guarda 1 semana)
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
        ?? throw new InvalidOperationException("JwtSettings no configurado.");

    var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>()
        ?? throw new InvalidOperationException("RabbitMQ no configurado.");

    // 2. Base de Datos
    builder.Services.AddDbContext<AssetDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CondoNet.Asset.Infrastructure")));

    builder.Services.AddHttpContextAccessor();

    // 3. Inyección de Dependencias
    // Servicios
    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<IAssetService, AssetService>();
    builder.Services.AddScoped<IUnitService, UnitService>();

    // 4. OpenAPI / Swagger
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(s =>
    {
        s.SwaggerDoc("v1", new OpenApiInfo { Title = "HabitApp Asset API", Version = "v1" });

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

        s.AddSecurityRequirement(d => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", d)] = [],
            [new OpenApiSecuritySchemeReference("ApiKey", d)] = []
        });
    });

    // 5. MassTransit con RabbitMQ
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqSettings.Host, (ushort)rabbitMqSettings.Port, "/", h =>
            {
                h.Username(rabbitMqSettings.Username);
                h.Password(rabbitMqSettings.Password);
            });
        });
    });

    // 6. Autenticación JWT
    var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

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
        .AddPolicy("RequireAssetRole", p => p.RequireRole("ADMIN", "AssetManager"))
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

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "SQL Server");

    var app = builder.Build();

    // 7. Pipeline de Middleware
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "HabitApp Asset API V1");
        c.RoutePrefix = "swagger";
    });

    // Health check endpoint
    app.MapHealthChecks("/health");

    // app.UseHttpsRedirection();

    // CORREGIDO: Autenticación PRIMERO para poder poblar el contexto de usuario
    app.UseAuthentication();
    app.UseAuthorization();

    // CORREGIDO: Movido después de la autenticación para que no dé siempre anónimo
    app.Use(async (context, next) =>
    {
        // 💡 Ajustado: Mapeado a "CondoId" en mayúsculas como viene en tu token JWT
        var condoId = context.User.FindFirst("CondoId")?.Value ?? "N/A";
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

        if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        using (Serilog.Context.LogContext.PushProperty("CondoId", condoId))
        using (Serilog.Context.LogContext.PushProperty("UserId", userId))
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.Headers.Append("X-Correlation-ID", correlationId);

            await next();
        }
    });

    // El middleware de ApiKey se ejecuta con los logs estructurados listos
    app.UseMiddleware<ApiKeyMiddleware>();

    // 4. Mapeo de Minimal APIs
    app.MapAssetEndpoints();
    app.MapCommonAssetEndpoints();
    app.MapCondominiumEndpoints();
    app.MapCriticalEquipmentEndpoints();
    app.MapTowerEndpoints();
    app.MapUnitEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Asset service falló en el arranque.");
}
finally
{
    Log.CloseAndFlush();
}
