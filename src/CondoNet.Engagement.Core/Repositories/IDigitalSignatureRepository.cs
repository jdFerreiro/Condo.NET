using CondoNet.Engagement.Core.Entities;

namespace CondoNet.Engagement.Core.Repositories
{
    public interface IDigitalSignatureRepository
    {
        Task<DigitalSignature?> GetByIdAsync(Guid id);
        Task<IEnumerable<DigitalSignature>> GetAllByDocumentAsync(Guid documentId);
        Task AddAsync(DigitalSignature signature);
        Task UpdateAsync(DigitalSignature signature);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}