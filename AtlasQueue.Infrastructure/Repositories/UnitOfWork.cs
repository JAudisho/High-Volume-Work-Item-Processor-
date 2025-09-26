using AtlasQueue.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AtlasQueue.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _db;
    public UnitOfWork(DbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}