using System.Collections.Frozen;
using GW2EIGW2API.Interfaces;
using GW2EIGW2API.Models;
using Microsoft.EntityFrameworkCore;

namespace GW2EIGW2API.GW2DB;

public sealed class CachedGW2Repository<T> : IGW2DbRepository<T> where T : GW2APIBaseItem
{
    public CachedGW2Repository(IDbContextFactory<AppDbContext> factory)
    {
        AppDbContext context = factory.CreateDbContext();
        _items = context.Set<T>().AsNoTracking().ToFrozenDictionary(x => x.Id);
    }

    private readonly FrozenDictionary<long, T> _items;

    public T? GetById(long id)
    {
        return _items.GetValueOrDefault(id);
    }

    public Task<T> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return Task.FromResult(GetById(id));
    }
}
