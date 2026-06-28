namespace CondoNET.Financial.Worker;

public interface IMatchingEngine
{
    Task MatchPaymentsAsync(CancellationToken cancellationToken = default);
}
