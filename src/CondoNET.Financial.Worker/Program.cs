using CondoNET.Financial.Worker;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// MassTransit (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitSettings = builder.Configuration.GetSection("RabbitMqSettings").Get<CondoNet.Shared.Settings.RabbitMqSettings>();
        cfg.Host(rabbitSettings.Host, (ushort)rabbitSettings.Port, rabbitSettings.VirtualHost, h =>
        {
            h.Username(rabbitSettings.Username);
            h.Password(rabbitSettings.Password);
        });
    });
});

// Nethereum y configuración de blockchain
builder.Services.Configure<BlockchainSettings>(builder.Configuration.GetSection("BlockchainSettings"));
builder.Services.AddSingleton<IBlockchainService, BlockchainService>();

var host = builder.Build();
host.Run();
