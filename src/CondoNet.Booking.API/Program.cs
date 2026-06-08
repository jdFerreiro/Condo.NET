using CondoNet.Booking.API.Endpoints;
using CondoNet.Booking.Core.Events;
using CondoNet.Booking.Core.Repositories;
using CondoNet.Booking.Core.Services;
using CondoNet.Booking.Infrastructure.Events;
using CondoNet.Booking.Infrastructure.Persistence;
using CondoNet.Booking.Infrastructure.Repositories;
using CondoNet.Booking.Infrastructure.Services;
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
    Log.Information("Iniciando el microservicio Booking Service de CondoNET...");

    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Evita spam de logs internos de .NET
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BookingService") // Identifica que este log es de Booking
        .WriteTo.Console()
        .WriteTo.File("Logs/log-.txt",
            rollingInterval: RollingInterval.Day, // Un archivo por día: log-20240321.txt
            retainedFileCountLimit: 7,            // Borra logs viejos automáticamente (guarda 1 semana)
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    builder.Host.UseSerilog();

    string redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionStrings").Value ?? "redis:6379";
    if (string.IsNullOrWhiteSpace(redisConnectionString))
        throw new InvalidOperationException("Redis:ConnectionStrings no configurado. Usa User Secrets para agregarlo en desarrollo.");

    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
        StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));

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

    builder.Services.AddDbContext<BookingDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CondoNet.Booking.Infrastructure")));

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<IBookingAvailabilityService, BookingAvailabilityService>();
    builder.Services.AddScoped<IBookingRepository, BookingRepository>();
    builder.Services.AddScoped<IBookingService, CondoNet.Booking.Core.Services.BookingService>();
    builder.Services.AddScoped<IEventPublisher, EventPublisher>();

    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(s =>
    {
        s.SwaggerDoc("v1", new OpenApiInfo { Title = "CondoNet Booking API", Version = "v1" });

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
        .AddPolicy("RequireAdminRole", policy => policy.RequireRole("ADMIN"))
        .AddPolicy("RequireBookingRole", policy => policy.RequireRole("ADMIN", "BookingManager"))
        .AddPolicy("RequiredAnyRole", policy => policy.RequireRole("ADMIN", "Manager", "User"));

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

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "CondoNet Booking API V1");
        c.RoutePrefix = "swagger";
    });

    // HTTP Request Pipeline

    // 🔥 CORREGIDO: Ejecutamos autenticación primero para poblar el ClaimsPrincipal (context.User)
    app.UseAuthentication();
    app.UseAuthorization();

    // 🔥 CORREGIDO: Movido abajo de la autenticación para evitar que CondoId devuelva siempre "N/A"
    app.Use(async (context, next) =>
    {
        // 💡 Ajustado: Cambiado "condo_id" a "CondoId" en mayúsculas para alinearse con tus claims
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

    app.UseMiddleware<ApiKeyMiddleware>();

    app.MapBookingEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Booking service falló en el arranque.");
}
finally
{
    Log.CloseAndFlush();
}
