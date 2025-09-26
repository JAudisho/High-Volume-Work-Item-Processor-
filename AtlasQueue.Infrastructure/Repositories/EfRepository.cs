using AtlasQueue.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AtlasQueue.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _db;
    public EfRepository(DbContext db) => _db = db;

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _db.Set<T>().AddAsync(entity, ct);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await _db.Set<T>().AddRangeAsync(entities, ct);

    public async Task<T?> GetAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _db.Set<T>().FirstOrDefaultAsync(predicate, ct);

    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();

    public void Update(T entity) => _db.Set<T>().Update(entity);
}