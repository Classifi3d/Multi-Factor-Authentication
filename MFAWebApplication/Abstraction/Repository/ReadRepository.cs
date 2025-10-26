using MFAWebApplication.Context;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Abstraction.Repository;
public class ReadRepository<TEntity> : IReadRepository<TEntity> where TEntity : class
{
    private readonly ReadDbContext _context;

    public ReadRepository(ReadDbContext context)
    {
        _context = context;
    }

    public IQueryable<TEntity> Query() { 
        return _context.Set<TEntity>().AsNoTracking();
    }

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }
}