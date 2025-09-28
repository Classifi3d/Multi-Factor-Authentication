using AuthenticationWebApplication.Repository;
using MFAWebApplication.Repository;

namespace MFAWebApplication.Abstraction;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : class;
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync( CancellationToken cancellationToken = default);
}
