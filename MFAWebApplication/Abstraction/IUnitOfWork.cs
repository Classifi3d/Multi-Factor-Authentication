namespace MFAWebApplication.Abstraction;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync( CancellationToken cancellationToken = default);
}
