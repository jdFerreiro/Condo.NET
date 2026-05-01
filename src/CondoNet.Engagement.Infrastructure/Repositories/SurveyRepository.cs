using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Infrastructure.Repositories
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly Persistence.EngagementDbContext _context;
        public SurveyRepository(Persistence.EngagementDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Survey survey) => await _context.Surveys.AddAsync(survey);
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Surveys.FindAsync(id);
            if (entity != null) _context.Surveys.Remove(entity);
        }
        public async Task<IEnumerable<Survey>> GetAllAsync() => await _context.Surveys.ToListAsync();
        public async Task<Survey?> GetByIdAsync(Guid id) => await _context.Surveys.FindAsync(id);
        public async Task UpdateAsync(Survey survey) => _context.Surveys.Update(survey);
        public async Task<bool> ExistsAsync(Guid id) => await _context.Surveys.AnyAsync(x => x.Id == id);
    }
}