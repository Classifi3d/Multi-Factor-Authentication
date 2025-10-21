using MFAWebApplication.Repository;

namespace MFAWebApplication.Abstraction;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync( CancellationToken cancellationToken = default);
}
