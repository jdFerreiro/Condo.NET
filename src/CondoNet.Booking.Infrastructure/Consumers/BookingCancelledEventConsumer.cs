using CondoNet.Shared.Booking.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CondoNet.Booking.Infrastructure.Consumers
{
    public class BookingCancelledEventConsumer(ILogger<BookingCancelledEventConsumer> logger) : IConsumer<BookingCancelledIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<BookingCancelledIntegrationEvent> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Recibido evento de cancelación de reserva: {EventId} para la reserva {BookingId} en Condominio {CondoId}",
                    message.EventId, message.BookingId, message.CondoId);
            }

            // ====================================================================
            // REGLA DE NEGOCIO: Reversión Contable en Accounting
            // ====================================================================
            // 1. Invocar al repositorio contable correspondiente (ej: _creditNoteRepository).
            // 2. Verificar si el cobro original de la reserva ya fue facturado o pagado.
            // 3. Generar una Nota de Crédito o disminuir el saldo pendiente del copropietario (message.UserId)
            //    por el monto exacto reembolsable (message.Price).

            // Simulación del proceso contable asíncrono
            await Task.Delay(100);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Nota de crédito aplicada con éxito al usuario {UserId} por un valor de {Price}",
                        message.UserId, message.Price);
            }
        }
    }
}
