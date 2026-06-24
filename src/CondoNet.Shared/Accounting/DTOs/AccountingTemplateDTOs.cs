namespace CondoNet.Shared.Accounting.DTOs
{
    public record CreateTemplateRequest(int EventType, string Name, string Description, List<CreateTemplateRuleRequest> Rules);
    public record CreateTemplateRuleRequest(Guid AccountId, int Movement, decimal PercentageFactor);
    public record TemplateResponse(Guid Id, int EventType, string Name, string Description, List<TemplateRuleDto> Rules);
    public record TemplateRuleDto(Guid Id, Guid AccountId, string AccountName, string AccountCode, int Movement, decimal PercentageFactor);
}
