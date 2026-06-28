using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Engagement.Infrastructure.Repositories
{
    public class DigitalSignatureRepository : IDigitalSignatureRepository
    {
        private readonly Persistence.EngagementDbContext _context;
        public DigitalSignatureRepository(Persistence.EngagementDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(DigitalSignature signature) => await _context.DigitalSignatures.AddAsync(signature);
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.DigitalSignatures.FindAsync(id);
            if (entity != null) _context.DigitalSignatures.Remove(entity);
        }
        public async Task<IEnumerable<DigitalSignature>> GetAllByDocumentAsync(Guid documentId) => await _context.DigitalSignatures.Where(x => x.DocumentId == documentId).ToListAsync();
        public async Task<DigitalSignature?> GetByIdAsync(Guid id) => await _context.DigitalSignatures.FindAsync(id);
        public async Task UpdateAsync(DigitalSignature signature) => _context.DigitalSignatures.Update(signature);
        public async Task<bool> ExistsAsync(Guid id) => await _context.DigitalSignatures.AnyAsync(x => x.Id == id);
    }
}