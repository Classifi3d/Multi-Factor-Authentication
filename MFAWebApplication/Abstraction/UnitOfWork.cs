
using AuthenticationWebApplication.Context;
using AuthenticationWebApplication.Repository;
using MFAWebApplication.Repository;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Abstraction;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork( 
        ApplicationDbContext dbContext,
        IUserRepository userRepository )
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
    }
    public IUserRepository Users => _userRepository;

    public IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : class
    {
        var type = typeof(TEntity);

        if ( _repositories.TryGetValue(type, out var repo) )
        {
            return (IRepository<TEntity, TId>) repo;
        }

        var newRepo = new Repository<TEntity, TId>(_dbContext);
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
