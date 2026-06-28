// CondoNet.Booking.Infrastructure.Messaging.Consumers/UserDebtStatusChangedConsumer.cs
using CondoNet.Booking.Core.Entities;
using CondoNet.Booking.Core.Repositories;
using CondoNet.Shared.Booking.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CondoNet.Booking.Infrastructure.Consumers
{
    public class UserDebtStatusChangedConsumer(
        IBookingRepository repository,
        ILogger<UserDebtStatusChangedConsumer> logger) : IConsumer<UserDebtStatusChangedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<UserDebtStatusChangedIntegrationEvent> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Sincronizando estado de deuda para Usuario {UserId} en Condo {CondoId}. Bloqueado: {Status}",
                        message.UserId, message.CondoId, message.HasOverdueDebt);
            }

            var debtStatus = new UserDebtStatus
            {
                Id = Guid.NewGuid(),
                CondoId = message.CondoId,
                UserId = message.UserId,
                IsBlocked = message.HasOverdueDebt,
                UpdatedAt = message.OccurredOn
            };

            await repository.SaveDebtStatusAsync(debtStatus);
        }
    }
}
