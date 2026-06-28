// CondoNet.Accounting.Infrastructure.Messaging/Consumers/BookingCreatedEventConsumer.cs
using CondoNet.Shared.Booking.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CondoNet.Booking.Infrastructure.Consumers
{
    public class BookingCreatedEventConsumer(ILogger<BookingCreatedEventConsumer> logger) : IConsumer<BookingCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<BookingCreatedIntegrationEvent> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Procesando cobro de la reserva {BookingId} (Condominio: {CondoId})",
                message.BookingId, message.CondoId);
            }

            // ====================================================================
            // INTEGRACIÓN REGLA FINANCIERA: Cargo Total vs Anticipo Requerido
            // ====================================================================
            decimal totalACobrar = message.TotalPrice;
            decimal anticipoObligatorio = message.RequiredDepositAmount;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Detalle financiero - Costo Total: ${Total}, Depósito Requerido (Mínimo): ${Deposit}",
                        totalACobrar, anticipoObligatorio);
            }

            // 1. Invocar a la lógica de tu dominio contable (ej: _invoiceService o _accountRepository)
            // 2. Persistir la factura principal en estado 'Pendiente' por el valor de 'totalACobrar'
            // 3. Crear un desglose o requerimiento de pago inmediato ('PaymentIntent') enlazado 
            //    a la pasarela de pagos interna por el valor exclusivo de 'anticipoObligatorio'

            if (anticipoObligatorio > 0)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Generando orden de pago prioritario para el usuario {UserId} por ${Deposit} para asegurar el Asset {AssetId}",
                    message.UserId, anticipoObligatorio, message.AssetId);
                }

                // TODO: Persistir el desglose contable del anticipo
            }
            else
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("La reserva no requiere depósito inicial (Porcentaje configurado = 0%). Se factura al 100% de forma ordinaria. BookingId: {BookingId}", message.BookingId);
                }
            }

            // Simulación asíncrona de persistencia en la base de datos de contabilidad
            await Task.Delay(100);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Estructura de cuenta por cobrar asentada con éxito para la reserva {BookingId}.", message.BookingId);
            }
        }
    }
}
