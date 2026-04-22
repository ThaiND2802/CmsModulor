using System.Linq.Expressions;

namespace CommerceCore.Infrastructure.Persistence.Abstractions;

public interface IRepository<TEntity>
    where TEntity : class
{
    IQueryable<TEntity> Query();

    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);
}
