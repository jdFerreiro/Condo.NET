using CondoNet.Booking.API.Endpoints;
using CondoNet.Booking.API.Middleware;
using CondoNet.Booking.Core.Events;
using CondoNet.Booking.Core.Repositories;
using CondoNet.Booking.Core.Validators;
using CondoNet.Booking.Infrastructure.Consumers;
using CondoNet.Booking.Infrastructure.Messaging;
using CondoNet.Booking.Infrastructure.Persistence;
using CondoNet.Booking.Infrastructure.Repositories;
using CondoNet.Booking.Infrastructure.Services;
using CondoNet.Shared.Booking.DTOs;
using CondoNet.Shared.Interfaces;
using CondoNet.Shared.Middleware;
using CondoNet.Shared.Services;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Security.Claims;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando el microservicio Booking Service de CondoNET...");
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BookingService")
        .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
        .WriteTo.File(new Serilog.Formatting.Compact.CompactJsonFormatter(), "Logs/log-.json", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
        .CreateLogger();

    builder.Host.UseSerilog();

    // ==========================================
    // 1. INFRAESTRUCTURA BASE Y SEGURIDAD
    // ==========================================

    // Requerido para que ITenantService acceda al HttpContext actual y extraiga los Claims
    builder.Services.AddHttpContextAccessor();

    // Registro del servicio de Tenant como Scoped (una instancia por cada petición HTTP)
    builder.Services.AddScoped<ITenantService, TenantService>();


    // ==========================================
    // 2. PERSISTENCIA Y MULTI-TENANCY
    // ==========================================

    // Registro del DbContext de Entity Framework Core
    builder.Services.AddDbContext<BookingDbContext>((serviceProvider, options) =>
    {
        // Recupera la cadena de conexión desde appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        options.UseSqlServer(connectionString, sqlOptions =>
        {
            // Define el ensamblado donde se guardarán las migraciones
            sqlOptions.MigrationsAssembly("CondoNet.Booking.Infrastructure");
        });
    });


    // ==========================================
    // 3. REPOSITORIOS Y LÓGICA DE NEGOCIO
    // ==========================================

    // Registro del repositorio para que la capa de Application pueda consumirlo
    builder.Services.AddScoped<IBookingRepository, BookingRepository>();
    // Registro de FluentValidation
    builder.Services.AddScoped<IValidator<CreateBookingRequest>, CreateBookingRequestValidator>();
    // Registro del Application Service
    builder.Services.AddScoped<BookingApplicationService>();


    // ==========================================
    // 4. CONFIGURACIÓN ADICIONAL DEL API
    // ==========================================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ==========================================
    // REGISTRO DE MANEJO DE EXCEPCIONES GLOBALES
    // ==========================================
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails(); // Agrega soporte nativo para ProblemDetails

    // ==========================================
    // CONFIGURACIÓN DE MASSTRANSIT + RABBITMQ + OUTBOX
    // ==========================================
    builder.Services.AddMassTransit(x =>
    {

        x.AddConsumer<BookingDepositPaidConsumer>();
        x.AddConsumer<BookingExpirationConsumer>();
        x.AddConsumer<UserDebtStatusChangedConsumer>();

        // 1. Configurar el almacenamiento del Outbox usando Entity Framework Core
        x.AddEntityFrameworkOutbox<BookingDbContext>(o =>
        {
            // Almacena los mensajes en la misma transacción que tu lógica de negocio
            o.UseSqlServer();
            // Habilita el Background Bus Delivery para despachar los mensajes de forma asíncrona
            o.UseBusOutbox();
            // 2. CONFIGURAR PARÁMETROS DE RETENCIÓN Y BARRIDO
            // Frecuencia con la que el Worker despertará a limpiar la tabla en SQL (ej: cada 5 minutos)
            o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
        });

        // 2. Definir el transporte: RabbitMQ
        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitUri = builder.Configuration.GetValue<string>("RabbitMQ:Uri") ?? "rabbitmq://localhost";
            var user = builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
            var pass = builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

            cfg.Host(new Uri(rabbitUri), h =>
            {
                h.Username(builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest");
                h.Password(builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest");
            });

            cfg.ConfigureEndpoints(context);

            cfg.ReceiveEndpoint("accounting-booking-charges", e =>
            {
                // Política de reintentos: Si la base de datos de contabilidad está bloqueada,
                // reintenta 3 veces con intervalos de 5 segundos antes de mandar a la cola de error (DLQ)
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                e.ConfigureConsumer<BookingCreatedEventConsumer>(context);
            });

            cfg.ReceiveEndpoint("accounting-booking-reversions", e =>
            {
                // Política de resiliencia: reintentar ante fallos de concurrencia en la DB
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                // Conecta la cola de RabbitMQ con la lógica del nuevo Consumer
                e.ConfigureConsumer<BookingCancelledEventConsumer>(context);
            });

            // Cola exclusiva de Booking para escuchar eventos de Contabilidad
            cfg.ReceiveEndpoint("booking-user-debt-sync", e =>
            {
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                e.ConfigureConsumer<UserDebtStatusChangedConsumer>(context);
            });

            cfg.ReceiveEndpoint("booking-payment-confirmation-sync", e =>
            {
                // Resiliencia: Reintentar 3 veces si la tabla de bookings está bloqueada temporalmente
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                e.ConfigureConsumer<BookingDepositPaidConsumer>(context);
            });

            cfg.ConfigureEndpoints(context);
        });
    });

    // Registrar tu abstracción limpia del Core para que apunte a MassTransit
    builder.Services.AddScoped<IEventBus, MassTransitEventBus>();

    var app = builder.Build();

    app.UseExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "CondoNet Booking API V1"); c.RoutePrefix = "swagger"; });

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Use(async (context, next) =>
    {
        var orgClaim = context.User.FindFirst("OrganizationId")?.Value ?? "N/A";
        var condoClaim = context.User.FindFirst("CondoId")?.Value ?? "N/A";
        var userClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

        using (Serilog.Context.LogContext.PushProperty("OrganizationId", orgClaim))
        using (Serilog.Context.LogContext.PushProperty("CondoId", condoClaim))
        using (Serilog.Context.LogContext.PushProperty("UserId", userClaim))
        {
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
