using System.Linq.Expressions;

namespace AtlasQueue.Domain.Abstractions;

public interface IRepository<T> where T : class
{
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    IQueryable<T> Query();
    void Update(T entity);
}