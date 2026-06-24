using CondoNet.BlockChain.Core;
using CondoNet.Shared.Settings;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using RabbitMQ.Client;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Defensive: ensure Kestrel endpoints have URLs configured to avoid
// an InvalidOperationException when the configuration contains an
// endpoint entry without the required 'Url' value.
var httpsUrl = builder.Configuration["Kestrel:Endpoints:Https:Url"];

// Configuración de Redis
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisSettings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisSettings>>().Value;
    return ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
});

// Configuración de RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

// 2. Registro de la Factoría de Conexiones (Requerido por MassTransit 9 para gestionar reconexiones)
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var rabbitSettings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
    return new ConnectionFactory()
    {
        Uri = new Uri(rabbitSettings.ConnectionString),
        AutomaticRecoveryEnabled = true // Habilita la recuperación automática de canales rotos
    };
});

// 3. Registro del IConnection unificado (Resuelve el error de compilación utilizando la firma asíncrona nativa)
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();

    // Resolvemos la tarea de forma segura en el arranque del contenedor de dependencias
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// Configuración de BlockChain
builder.Services.Configure<BlockchainSettings>(builder.Configuration.GetSection("BlockChain"));
builder.Services.AddSingleton<Web3>(sp =>
{
    var blockchainSettings = sp.GetRequiredService<IOptions<BlockchainSettings>>().Value;
    return new Web3(blockchainSettings.RpcUrl);
});

// Agrega YARP
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsParaFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7110", "https://localhost:7100/") // URL(s) exacta(s) de tu Frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Crucial si usas cookies o Identity
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();

app.UseRouting();
app.UseStaticFiles(); // <-- Esto es clave para Swagger UI
app.UseCors("CorsParaFrontend");

app.MapReverseProxy();

// Endpoints básicos
app.MapGet("/", () => "Gateway activo");
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
