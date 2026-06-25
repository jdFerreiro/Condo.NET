using CondoNet.Accounting.Api.Endpoints;
using CondoNet.Accounting.Core.Interfaces.Repositories;
using CondoNet.Accounting.Infrastructure;
using CondoNet.Accounting.Infrastructure.Persistence;
using CondoNet.Accounting.Infrastructure.Repositories;
using CondoNet.Shared.Handlers;
using CondoNet.Shared.Interfaces;
using CondoNet.Shared.Middleware;
using CondoNet.Shared.Services;
using CondoNet.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando el microservicio Accounting Service de HabitApp...");
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "AccountingService")
        .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
        .WriteTo.File(new Serilog.Formatting.Compact.CompactJsonFormatter(), "Logs/log-.json", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings no configurado.");
    var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>() ?? throw new InvalidOperationException("RabbitMQ no configurado.");

    // =========================================================================
    // PERSISTENCIA Y CONTEXTOS DE BASE DE DATOS
    // =========================================================================
    builder.Services.AddDbContext<AccountingDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("CondoNet.Accounting.Infrastructure")));

    // Mapeo Crítico: Resuelve el requerimiento de DbContext de tus constructores primarios
    builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AccountingDbContext>());

    // =========================================================================
    // MENSAJERÍA Y INFRAESTRUCTURA (MassTransit, RabbitMQ, Cuentas, Seeder)
    // =========================================================================
    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<InternalHttpGatewayHandler>();

    // =========================================================================
    // REPOSITORIOS Y SERVICIOS AUXILIARES
    // =========================================================================
    builder.Services.AddScoped<IAccountingEntryRepository, AccountingEntryRepository>();
    builder.Services.AddScoped<IAccountingTransactionRepository, AccountingTransactionRepository>();
    builder.Services.AddScoped<IAccountRepository, AccountRepository>();
    builder.Services.AddScoped<ITenantService, TenantService>();

    // NOTA: Si 'AccountingAutomatonService' es una clase vieja o duplicada que creamos
    // antes de unificar el autómata dentro de la clase parcial 'AccountingService', 
    // debes comentar o eliminar su registro aquí para evitar colisiones en la validación:
    // builder.Services.AddScoped<IAccountingAutomatonService, AccountingAutomatonService>();

    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(s =>
    {
        s.SwaggerDoc("v1", new OpenApiInfo { Title = "HabitApp Accounting API", Version = "v1" });
        s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "Bearer", BearerFormat = "JWT", In = ParameterLocation.Header, Description = "Escribe el token JWT." });
        s.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme { Name = "X-Api-Key", Type = SecuritySchemeType.ApiKey, In = ParameterLocation.Header, Description = "Ingresa tu API Key." });
        s.AddSecurityRequirement(document => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("bearer", document)] = [], [new OpenApiSecuritySchemeReference("ApiKey", document)] = [] });
    });

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
        x.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = jwtSettings.Issuer, ValidAudience = jwtSettings.Audience, IssuerSigningKey = new SymmetricSecurityKey(key) };
    });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdminRole", policy => policy.RequireRole("ADMIN"))
        .AddPolicy("RequireAccountingRole", policy => policy.RequireRole("ADMIN", "AccountingManager"))
        .AddPolicy("RequiredAnyRole", policy => policy.RequireRole("ADMIN", "Manager", "User"));

    builder.Services.AddHttpClient("AuthService", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("AuthServiceUrl") ?? "http://localhost:8010/");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<InternalHttpGatewayHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });

    builder.Services.AddHealthChecks().AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "SQL Server");

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "HabitApp Accounting API V1");
        c.RoutePrefix = "swagger";
    });

    app.MapHealthChecks("/health");

    // Pipeline ordenado jerárquicamente
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.Use(async (context, next) =>
    {
        // Leemos las claims de forma segura (sin lanzar excepciones si no existen)
        var orgClaim = context.User.FindFirst("OrganizationId")?.Value ?? "N/A";
        var condoClaim = context.User.FindFirst("CondoId")?.Value ?? "N/A";
        var userClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

        // Indexamos las propiedades en la estructura JSON de Serilog
        using (Serilog.Context.LogContext.PushProperty("OrganizationId", orgClaim))
        using (Serilog.Context.LogContext.PushProperty("CondoId", condoClaim))
        using (Serilog.Context.LogContext.PushProperty("UserId", userClaim))
        {
            await next();
        }
    });

    app.UseMiddleware<ApiKeyMiddleware>();

    app.MapAccountEndpoints();
    app.MapJournalEndpoints();
    app.MapReportEndpoints();
    app.MapTemplateEndpoints();

    app.Run();
}
catch (Exception ex)
{
    // 💡 FILTRADO DE SEGURIDAD PARA HERRAMIENTAS DE EF CORE / MIGRACIONES
    // Si la excepción fue causada por la detención intencional del Host de diseño, la ignoramos.
    if (ex.GetType().Name == "HostAbortedException")
    {
        Log.Information("El host se detuvo de forma controlada (Inspección de diseño de EF Core / Migraciones).");
    }
    else
    {
        Log.Fatal(ex, "Accounting service falló en el arranque real en producción.");
    }
}
finally
{
    Log.CloseAndFlush();
}
