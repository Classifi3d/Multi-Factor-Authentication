
using AuthenticationWebApplication.Context;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Context;

namespace MFAWebApplication.Abstraction.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly WriteDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

    public UnitOfWork(
        WriteDbContext dbContext )
    {
        _dbContext = dbContext;
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if ( _repositories.TryGetValue(type, out var repo) )
        {
            return (IRepository<TEntity>) repo;
        }

        var newRepo = new Repository<TEntity>(_dbContext);
        _repositories[type] = newRepo;
        return newRepo;
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
