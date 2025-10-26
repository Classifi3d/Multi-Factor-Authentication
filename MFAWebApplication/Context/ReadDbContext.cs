using AuthenticationWebApplication.Enteties;
using MFAWebApplication.Enteties;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Context;

public class ReadDbContext : DbContext
{

    public ReadDbContext( DbContextOptions<ReadDbContext> options ) : base(options) { }

    public DbSet<UserReadModel> Users => Set<UserReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

}