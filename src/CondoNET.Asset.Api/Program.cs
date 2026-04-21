using CondoNet.Asset.Api.Endpoints;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infraestructure.Persistence;
using CondoNet.Asset.Infraestructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

    Log.Information("Iniciando el microservicio Asset Service de CondoNET...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration) // Lee de appsettings.json
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());


    // 1. Servicios de Infraestructura
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "CondoNet - Asset Service", Version = "v1" });
    });
    builder.Services.AddHttpContextAccessor();

    // 2. Base de Datos y Multi-tenancy
    builder.Services.AddDbContext<AssetDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<IAssetService, AssetService>();

    builder.Services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"]!);
                h.Password(builder.Configuration["RabbitMq:Password"]!);
            });

            cfg.ConfigureEndpoints(context);
        });
    });

    // 3. Seguridad (JWT)
    builder.Services.AddAuthentication().AddJwtBearer();
    builder.Services.AddAuthorization();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // 4. Mapeo de Minimal APIs
    app.MapAssetEndpoints();

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