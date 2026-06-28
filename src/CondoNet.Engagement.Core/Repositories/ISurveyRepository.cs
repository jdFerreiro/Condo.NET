using CondoNet.Engagement.Core.Entities;

namespace CondoNet.Engagement.Core.Repositories
{
    public interface ISurveyRepository
    {
        Task<Survey?> GetByIdAsync(Guid id);
        Task<IEnumerable<Survey>> GetAllAsync();
        Task AddAsync(Survey survey);
        Task UpdateAsync(Survey survey);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}