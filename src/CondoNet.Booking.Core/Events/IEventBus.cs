namespace CondoNet.Booking.Core.Events
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T integrationEvent) where T : class;
    }
}