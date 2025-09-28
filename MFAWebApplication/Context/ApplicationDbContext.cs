using AuthenticationWebApplication.Enteties;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationWebApplication.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : base(options) { }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<User>().HasKey(x => x.Id);
        modelBuilder.Entity<User>().HasIndex(x => x.Id).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
    }

    public DbSet<User> User { get; set; }
}
