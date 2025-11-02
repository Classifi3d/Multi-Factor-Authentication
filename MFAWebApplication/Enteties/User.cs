using MFAWebApplication.Enteties;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationWebApplication.Enteties;


[Index(nameof(Id), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
[Table("User")]
public class User : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public string? MfaSecretKey { get; set; }
    public bool IsMfaEnabled { get; set; } = false;
    [ConcurrencyCheck]
    public ulong ConcurencyIndex { get; set; } = 1;
}
