namespace MFAWebApplication.Abstraction.Repository;
public interface IReadRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query();
}