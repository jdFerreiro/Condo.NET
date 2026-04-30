namespace CondoNet.Shared.DTOs.Payment;

public class WebhookEventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
}
