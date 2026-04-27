using System.Threading.Tasks;
using MassTransit;
using CondoNet.Shared.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace CondoNet.Notifications.Worker.Consumers;

public class PaymentNotificationConsumer : IConsumer<PaymentNotificationEvent>
{
    private readonly ILogger<PaymentNotificationConsumer> _logger;

    public PaymentNotificationConsumer(ILogger<PaymentNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentNotificationEvent> context)
    {
        var evt = context.Message;
        // Aquí va la lógica para enviar la notificación (push/email/etc.)
        _logger.LogInformation($"Notificación de pago validado enviada a {evt.ResidentEmail} por el pago {evt.PaymentId}.");
        await Task.CompletedTask;
    }
}
