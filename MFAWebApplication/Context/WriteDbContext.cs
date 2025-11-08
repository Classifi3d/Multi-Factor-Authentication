using AuthenticationWebApplication.Enteties;
using MFAWebApplication.Outbox;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Context;

public class WriteDbContext : DbContext
{
    public WriteDbContext( DbContextOptions<WriteDbContext> options ) : base(options) { }

    public DbSet<User> Users => Set<User>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ConcurrencyIndex++;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

}
