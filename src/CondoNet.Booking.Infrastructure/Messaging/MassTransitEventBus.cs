// CondoNet.Booking.Infrastructure.Messaging/MassTransitEventBus.cs
using CondoNet.Booking.Core.Events;
using MassTransit;

namespace CondoNet.Booking.Infrastructure.Messaging;

public class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent) where T : class
    {
        // MassTransit se encarga de serializar a JSON y enviar al Exchange/Topic correcto
        await publishEndpoint.Publish(integrationEvent);
    }
}
