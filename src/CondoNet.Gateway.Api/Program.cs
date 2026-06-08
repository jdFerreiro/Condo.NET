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
builder.Services.AddSingleton<IConnection>(sp =>
{
    var rabbitSettings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqSettings>>().Value;
    var factory = new ConnectionFactory()
    {
        Uri = new Uri(rabbitSettings.ConnectionString)
    };
    return factory.CreateConnection();
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
