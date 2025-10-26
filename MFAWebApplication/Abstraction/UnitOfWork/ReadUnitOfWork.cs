using AuthenticationWebApplication.Context;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Context;

namespace MFAWebApplication.Abstraction.UnitOfWork;

public class ReadUnitOfWork : IReadUnitOfWork
{
    private readonly ReadDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

    public ReadUnitOfWork(
        ReadDbContext dbContext )
    {
        _dbContext = dbContext;
    }

    public IReadRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if ( _repositories.TryGetValue(type, out var repo) )
        {
            return (IReadRepository<TEntity>) repo;
        }

        var newRepo = new ReadRepository<TEntity>(_dbContext);
        _repositories[type] = newRepo;
        return newRepo;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }


}
