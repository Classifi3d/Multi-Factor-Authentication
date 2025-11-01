using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MFAWebApplication.Enteties;


[Index(nameof(Id), IsUnique = true)]
[Table("UserReadModel")]
public class UserReadModel
{
    [Key]
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsMfaEnabled { get; set; }
    public long ConcurrencyVersion { get; set; }
}