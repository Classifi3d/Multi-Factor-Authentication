namespace MFAWebApplication.Repository;

public interface IRepository<TEntity,TId> where TEntity : class
{
    Task<TEntity?> GetByIdAsync( TId id, CancellationToken cancellationToken = default );
    Task AddAsync( TEntity entity, CancellationToken cancellationToken = default );
    void Update( TEntity entity );
    void Delete( TEntity entity );
    IQueryable<TEntity> GetAll();
}
