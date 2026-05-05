using CondoNet.BlockChain.Core;
using CondoNet.Shared.Settings;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using RabbitMQ.Client;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(80); // Solo HTTP
//     // Elimina o comenta la línea de HTTPS
//     // options.ListenAnyIP(443, listenOptions => listenOptions.UseHttps());
// });

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
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();

//app.UseHttpsRedirection();


// Endpoint básico para verificar que el gateway está activo
app.MapGet("/", () => "Gateway activo");

// Endpoint de health check
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
