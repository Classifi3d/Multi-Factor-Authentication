namespace MFAWebApplication.Enteties;
public class UserCreatedEvent
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsMfaEnabled { get; set; }
    public ulong ConcurrencyIndex { get; set; }

}
