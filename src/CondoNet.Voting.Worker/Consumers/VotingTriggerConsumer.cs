using System.Threading.Tasks;
using MassTransit;
using CondoNet.Shared.Events.Voting;
using Microsoft.Extensions.Logging;

namespace CondoNet.Voting.Worker.Consumers;

public class VotingTriggerConsumer : IConsumer<VotingTriggerEvent>
{
    private readonly ILogger<VotingTriggerConsumer> _logger;

    public VotingTriggerConsumer(ILogger<VotingTriggerConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VotingTriggerEvent> context)
    {
        var evt = context.Message;
        // Aquí va la lógica para iniciar el proceso de votación
        _logger.LogInformation($"Votación disparada por impugnación {evt.ImpugnationId} para el gasto {evt.OriginalDisbursementId}.");
        await Task.CompletedTask;
    }
}
