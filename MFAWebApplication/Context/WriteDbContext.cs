using AuthenticationWebApplication.Enteties;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Context;

public class WriteDbContext : DbContext
{
    public WriteDbContext( DbContextOptions<WriteDbContext> options ) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ConcurencyIndex++;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

}
