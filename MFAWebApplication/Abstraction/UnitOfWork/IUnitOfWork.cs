using MFAWebApplication.Abstraction.Repository;

namespace MFAWebApplication.Abstraction.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync( CancellationToken cancellationToken = default);
}
