namespace CondoNet.Shared.Events;

public record UserCreatedEvent
(
    Guid UserId,
    string Email,
    string FullName
);

public record UserContextSwitchedEvent(
    Guid UserId,
    Guid? OldCondoId,
    Guid? NewCondoId,
    DateTime Date
);