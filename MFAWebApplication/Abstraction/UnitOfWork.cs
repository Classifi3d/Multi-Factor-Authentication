
using AuthenticationWebApplication.Context;

namespace MFAWebApplication.Abstraction;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(ApplicationDbContext dbContext )
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync( CancellationToken cancellationToken = default )
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }


}
