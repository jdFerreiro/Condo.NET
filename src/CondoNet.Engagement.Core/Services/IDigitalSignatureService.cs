namespace CondoNet.Engagement.Core.Services
{
    public interface IDigitalSignatureService
    {
        Task ValidateAndSignAsync(Entities.DigitalSignature signature, Guid userId);
    }
}