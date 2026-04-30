using System.Threading.Tasks;

namespace CondoNet.Booking.Core.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : class;
    }
}