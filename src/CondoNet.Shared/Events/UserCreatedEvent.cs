namespace CondoNet.Shared.Events;

public record UserCreatedEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid OrganizationId { get; init; }
    public Guid? CondoId { get; init; }
}
