using System.Linq.Expressions;

namespace MFAWebApplication.Abstraction.Repository;
public interface IReadModelRepository<TEntity>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByPropertyAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector,
        TProperty value,
        CancellationToken cancellationToken = default);

    IQueryable<TEntity> GetAll();

    /// <summary>
    /// Delets only if the incoming ConcurrencyIndex is equal to the stored one.
    /// Returns true if a deletion has occurred.
    /// </summary>
    Task<bool> DeleteIfMatchingConcurrencyAsync(TEntity entity, CancellationToken cancellationToken = default);


    /// <summary>
    /// Upserts only if the incoming ConcurrencyIndex is greater than the stored one.
    /// Returns true if an insert or update occurred.
    /// </summary>
    Task<bool> UpsertIfNewerConcurrencyAsync(TEntity entity, CancellationToken cancellationToken = default);


}