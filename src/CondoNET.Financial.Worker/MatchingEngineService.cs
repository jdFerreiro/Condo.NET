namespace CondoNET.Financial.Worker;

public class MatchingEngineService : IMatchingEngine
{
    public async Task MatchPaymentsAsync(CancellationToken cancellationToken = default)
    {
        // Implementa aquí la lógica de conciliación automática/manual
        await Task.CompletedTask;
    }
}
