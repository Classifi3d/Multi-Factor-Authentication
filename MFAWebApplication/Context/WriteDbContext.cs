using AuthenticationWebApplication.Enteties;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Context;

public class WriteDbContext : DbContext
{
    public WriteDbContext( DbContextOptions<WriteDbContext> options ) : base(options) { }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<User>().HasKey(x => x.Id);
        modelBuilder.Entity<User>().HasIndex(x => x.Id).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
    }

    public DbSet<User> User { get; set; } = null!;

}
