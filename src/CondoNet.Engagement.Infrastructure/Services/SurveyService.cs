using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using CondoNet.Engagement.Core.Services;
using System;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Infrastructure.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _surveyRepository;

        public SurveyService(ISurveyRepository surveyRepository)
        {
            _surveyRepository = surveyRepository;
        }

        public async Task ValidateAndCreateAsync(Survey survey, Guid userId)
        {
            // Validar fechas
            if (survey.StartDate >= survey.EndDate)
                throw new InvalidOperationException("La fecha de inicio debe ser anterior a la de fin.");

            survey.IsActive = true;
            await _surveyRepository.AddAsync(survey);
        }

        public async Task ValidateAndRespondAsync(Guid surveyId, Guid userId, object response)
        {
            // Validar existencia
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                throw new InvalidOperationException("La encuesta no existe.");

            // Validar fechas
            var now = DateTime.UtcNow;
            if (now < survey.StartDate || now > survey.EndDate)
                throw new InvalidOperationException("La encuesta no está activa.");

            // Aquí deberías validar que el usuario no haya respondido antes (requiere entidad de respuesta)
            // Registrar respuesta (no implementado, placeholder)
            // await _surveyRepository.AddResponseAsync(surveyId, userId, response);
            await Task.CompletedTask;
        }
    }
}
