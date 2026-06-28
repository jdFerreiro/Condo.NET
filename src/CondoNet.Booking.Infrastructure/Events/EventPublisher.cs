using CondoNet.Booking.Core.Events;
using MassTransit;

namespace CondoNet.Booking.Infrastructure.Events
{
    public class EventPublisher(IPublishEndpoint publishEndpoint) : IEventBus
    {
        public async Task PublishAsync<T>(T @event) where T : class
        {
            await publishEndpoint.Publish(@event);
        }
    }
}