namespace CondoNet.Financial.Core.Services
{
    public interface IDataIntegrityValidatorService
    {
        Task ValidateMerkleRootsAsync(CancellationToken cancellationToken = default);
        // Valida splits registrados
        Task ValidateSplitsAsync(CancellationToken cancellationToken = default);

    }
}
