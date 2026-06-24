using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;

namespace CondoNet.Accounting.Core.Interfaces
{
    public interface IAccountingTemplateService
    {
        Task<Result<Guid>> CreateTemplateAsync(CreateTemplateRequest request);
        Task<Result<List<TemplateResponse>>> GetTemplatesByCondoAsync();
        Task<Result<bool>> DeleteTemplateAsync(Guid templateId);
    }
}
