using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using CondoNet.Engagement.Core.Services;
using System;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Infrastructure.Services
{
    public class DigitalSignatureService : IDigitalSignatureService
    {
        private readonly IDigitalSignatureRepository _digitalSignatureRepository;

        public DigitalSignatureService(IDigitalSignatureRepository digitalSignatureRepository)
        {
            _digitalSignatureRepository = digitalSignatureRepository;
        }

        public async Task ValidateAndSignAsync(DigitalSignature signature, Guid userId)
        {
            // Validar autenticación del usuario (asumido válido)
            signature.UserId = userId;
            signature.SignedAt = DateTime.UtcNow;
            await _digitalSignatureRepository.AddAsync(signature);
        }
    }
}
