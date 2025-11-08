using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Outbox;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Abstraction.UnitOfWork;

public class UnitOfWork<TContext> : IUnitOfWork, IDisposable
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
    private readonly List<object> _pendingOutboxEvent = new List<object>();

    public UnitOfWork(
        TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (_repositories.TryGetValue(type, out var repo))
        {
            return (IRepository<TEntity>)repo;
        }

        var newRepo = new Repository<TEntity>(_dbContext);
        _repositories[type] = newRepo;
        return newRepo;
    }

    public void AddOutboxEvent(object domainEvent)
    {
        _pendingOutboxEvent.Add(domainEvent);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var saveResult = await _dbContext.SaveChangesAsync(cancellationToken);

        if (_pendingOutboxEvent.Count > 0)
        {
            var serializedOutbox = _pendingOutboxEvent.Select(outboxEvent => new OutboxMessage
            {
                Type = outboxEvent.GetType().Name,
                Payload = MessagePack.MessagePackSerializer.Serialize(outboxEvent)
            }).ToList();

            await _dbContext.Set<OutboxMessage>().AddRangeAsync(serializedOutbox, cancellationToken);
            _pendingOutboxEvent.Clear();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return saveResult;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
