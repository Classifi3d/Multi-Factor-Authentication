using System.ComponentModel.DataAnnotations;
namespace MFAWebApplication.Outbox;

public class OutboxMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public bool Processed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
