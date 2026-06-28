// CondoNet.Booking.Infrastructure.Messaging.Consumers/BookingExpirationConsumer.cs
using CondoNet.Booking.Core.Repositories;
using CondoNet.Shared.Booking.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CondoNet.Booking.Infrastructure.Consumers
{
    public class BookingExpirationConsumer(
        IBookingRepository repository,
        ILogger<BookingExpirationConsumer> logger) : IConsumer<BookingExpirationReminder>
    {
        public async Task Consume(ConsumeContext<BookingExpirationReminder> context)
        {
            Guid bookingId = context.Message.BookingId;

            // Ignoramos filtros globales para procesos en segundo plano
            var booking = await repository.GetByIdAsync(bookingId, Guid.Empty);

            if (booking == null) return;

            // REGLA DE NEGOCIO: Transcurridas las X horas, si no se ha confirmado 
            // (lo que implica que no se pagó al menos el porcentaje de reserva requerido), se cancela.
            if (!booking.IsConfirmed)
            {
                logger.LogWarning("La reserva {BookingId} no cubrió el porcentaje mínimo de pago tras el tiempo límite. Cancelando...", bookingId);

                // Liberamos la franja horaria cambiando el estado o eliminando el registro
                booking.IsConfirmed = false;
                booking.UpdatedAt = DateTime.UtcNow;

                await repository.UpdateAsync(booking);

                // Notificar de forma asíncrona a Accounting que la reserva caducó para anular cargos pendientes
                await context.Publish(new BookingCancelledIntegrationEvent(
                    EventId: Guid.NewGuid(),
                    OccurredOn: DateTime.UtcNow,
                    BookingId: booking.Id,
                    CondoId: booking.CondoId,
                    UserId: booking.UserId,
                    Price: booking.Price
                ));
            }
            else
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("La reserva {BookingId} cumplió con el pago requerido a tiempo.", bookingId);
                }
            }
        }
    }
}
