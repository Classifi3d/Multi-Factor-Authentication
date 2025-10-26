using MFAWebApplication.Abstraction.Repository;

namespace MFAWebApplication.Abstraction.UnitOfWork;

public interface IReadUnitOfWork : IDisposable
{
    IReadRepository<TEntity> Repository<TEntity>() where TEntity : class;
}
