using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Repository;

public class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository( DbContext context )
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync( TId id, CancellationToken cancellationToken = default )
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync( TEntity entity, CancellationToken cancellationToken = default )
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update( TEntity entity )
    {
        _dbSet.Update(entity);
    }

    public void Delete( TEntity entity )
    {
        _dbSet.Remove(entity);
    }

    public IQueryable<TEntity> GetAll()
    {
        return _dbSet.AsQueryable();
    }
}
