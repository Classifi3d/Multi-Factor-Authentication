using MFAWebApplication.Context;
using MFAWebApplication.Enteties;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace MFAWebApplication.Abstraction.Repository;
public class ReadModelRepository<TEntity> : IReadModelRepository<TEntity> where TEntity : class, IReadModel
{
    private readonly IMongoCollection<TEntity> _collection;

    public ReadModelRepository(ReadDbContext readDbContext)
    {
        _collection = readDbContext.GetCollection<TEntity>(typeof(TEntity).Name);
    }

    public IQueryable<TEntity> GetAll() => _collection.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByPropertyAsync<TProperty>(
        Expression<Func<TEntity, TProperty>> propertySelector,
        TProperty value,
        CancellationToken cancellationToken = default)
    {
        var parameter = propertySelector.Parameters[0];
        var body = Expression.Equal(propertySelector.Body, Expression.Constant(value, typeof(TProperty)));
        var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        return await _collection.AsQueryable().FirstOrDefaultAsync(lambda, cancellationToken);
    }

    public async Task<bool> UpsertIfNewerConcurrencyAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var id = GetEntityId(entity);

        var filter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Eq("_id", id),
            Builders<TEntity>.Filter.Lt(nameof(IReadModel.ConcurrencyIndex), entity.ConcurrencyIndex)
        );

        var options = new ReplaceOptions { IsUpsert = true };

        var result = await _collection.ReplaceOneAsync(filter, entity, options, cancellationToken);

        if (!result.IsAcknowledged) return false;
        if (result.UpsertedId != null) return true;
        if (result.ModifiedCount > 0) return true;

        return false;
    }

    public async Task<bool> DeleteIfMatchingConcurrencyAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var id = GetEntityId(entity);

        var filter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Eq("_id", id),
            Builders<TEntity>.Filter.Eq(nameof(IReadModel.ConcurrencyIndex), entity.ConcurrencyIndex)
        );

        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    private static object? GetEntityId(TEntity entity)
    {
        var prop = typeof(TEntity).GetProperty("Id");
        return prop?.GetValue(entity);
    }

}

