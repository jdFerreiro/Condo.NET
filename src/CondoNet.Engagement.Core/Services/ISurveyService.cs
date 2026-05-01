namespace CondoNet.Engagement.Core.Services
{
    public interface ISurveyService
    {
        Task ValidateAndCreateAsync(Entities.Survey survey, Guid userId);
        Task ValidateAndRespondAsync(Guid surveyId, Guid userId, object response);
    }
}