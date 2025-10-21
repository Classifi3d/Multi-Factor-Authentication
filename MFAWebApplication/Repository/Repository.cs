using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MFAWebApplication.Repository;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository( DbContext context )
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync( object id, CancellationToken cancellationToken = default )
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<TEntity?> GetByPropertyAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector,
        TProperty value,
        CancellationToken cancellationToken = default )
    {
        var parameter = propertySelector.Parameters[0];
        var body = Expression.Equal(propertySelector.Body, Expression.Constant(value));
        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        return await _dbSet.FirstOrDefaultAsync(lambda, cancellationToken);
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
