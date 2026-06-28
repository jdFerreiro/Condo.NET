// CondoNet.Booking.Infrastructure.Messaging.Consumers/BookingDepositPaidConsumer.cs
using CondoNet.Booking.Core.Repositories;
using CondoNet.Shared.Booking.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CondoNet.Booking.Infrastructure.Consumers
{
    public class BookingDepositPaidConsumer(
        IBookingRepository repository,
        ILogger<BookingDepositPaidConsumer> logger) : IConsumer<BookingDepositPaidIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<BookingDepositPaidIntegrationEvent> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Recibida notificación de pago de Accounting para la reserva {BookingId} en el Condominio {CondoId}.",
                    message.BookingId, message.CondoId);
            }

            // 1. Recuperar la reserva asociando el CondoId explícito del evento
            var booking = await repository.GetByIdAsync(message.BookingId, message.CondoId);

            if (booking == null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError("No se encontró la reserva {BookingId} para el Condominio {CondoId}. No se pudo confirmar.",
                        message.BookingId, message.CondoId);
                }
                return;
            }

            // 2. Si ya está confirmada, ignoramos para asegurar la idempotencia del proceso
            if (booking.IsConfirmed)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("La reserva {BookingId} ya se encontraba confirmada previamente.", message.BookingId);
                }

                return;
            }

            // ====================================================================
            // REGLA DE NEGOCIO: Confirmación del espacio por pago exitoso
            // ====================================================================
            booking.IsConfirmed = true;
            booking.UpdatedAt = DateTime.UtcNow;

            // 3. Persistir la confirmación en la base de datos
            await repository.UpdateAsync(booking);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("La reserva {BookingId} ha sido CONFIRMADA exitosamente y queda protegida contra caducidad.",
                message.BookingId);
            }
        }
    }
}
