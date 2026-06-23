using CondoNet.Asset.Api.Endpoints;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Asset.Infrastructure.Services;
using CondoNet.Shared.Handlers;
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
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando el microservicio Asset Service de HabitApp...");
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "AssetService")
        .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
        .WriteTo.File(new Serilog.Formatting.Compact.CompactJsonFormatter(), "Logs/log-.json", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
        .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
    builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings no configurado.");
    var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>() ?? throw new InvalidOperationException("RabbitMQ no configurado.");

    builder.Services.AddDbContext<AssetDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("CondoNet.Asset.Infrastructure")));

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<InternalHttpGatewayHandler>();

    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<IAssetService, AssetService>();
    builder.Services.AddScoped<IUnitService, UnitService>();

    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(s =>
    {
        s.SwaggerDoc("v1", new OpenApiInfo { Title = "HabitApp Asset API", Version = "v1" });
        s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "Bearer", BearerFormat = "JWT", In = ParameterLocation.Header, Description = "Escribe el token JWT." });
        s.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme { Name = "X-Api-Key", Type = SecuritySchemeType.ApiKey, In = ParameterLocation.Header, Description = "Ingresa tu API Key." });
        s.AddSecurityRequirement(d => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("bearer", d)] = [], [new OpenApiSecuritySchemeReference("ApiKey", d)] = [] });
    });

    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqSettings.Host, (ushort)rabbitMqSettings.Port, "/", h => { h.Username(rabbitMqSettings.Username); h.Password(rabbitMqSettings.Password); });
        });
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
        .AddPolicy("RequireAssetRole", p => p.RequireRole("ADMIN", "AssetManager"))
        .AddPolicy("RequiredAnyRole", p => p.RequireRole("ADMIN", "Manager", "User"));

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
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "HabitApp Asset API V1"); c.RoutePrefix = "swagger"; });
    app.MapHealthChecks("/health");

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

    app.MapHealthChecks("/health");

    app.MapOrganizationEndpoints();
    app.MapCondominiumEndpoints();
    app.MapTowerAndUnitEndpoints();
    app.MapInventoryAssetEndpoints();

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
