using AuthenticationWebApplication.Enteties;
//using MFAWebApplication.Abstraction.Outbox;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Context;

public class WriteDbContext : DbContext
{
    public WriteDbContext( DbContextOptions<WriteDbContext> options ) : base(options) { }

    public DbSet<User> Users => Set<User>();

    //public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    //protected override void OnModelCreating( ModelBuilder modelBuilder )
    //{
    //    base.OnModelCreating(modelBuilder);

    //    modelBuilder.Entity<OutboxMessage>(b =>
    //    {
    //        b.HasKey(o => o.Id);
    //        b.Property(o => o.Type).IsRequired();
    //        b.Property(o => o.Payload).IsRequired();
    //        b.Property(o => o.Processed).HasDefaultValue(false);
    //        b.HasIndex(o => o.Processed);
    //    });
    //}

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
