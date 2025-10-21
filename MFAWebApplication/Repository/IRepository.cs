using AuthenticationWebApplication.Enteties;
using System.Linq.Expressions;

namespace MFAWebApplication.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync( object id, CancellationToken cancellationToken = default );
    Task<TEntity?> GetByPropertyAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector,
        TProperty value,
        CancellationToken cancellationToken = default
    );
    Task AddAsync( TEntity entity, CancellationToken cancellationToken = default );
    void Update( TEntity entity );
    void Delete( TEntity entity );
    IQueryable<TEntity> GetAll();
}
