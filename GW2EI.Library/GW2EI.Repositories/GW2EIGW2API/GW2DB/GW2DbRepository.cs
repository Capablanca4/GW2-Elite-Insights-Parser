using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GW2EIGW2API.GW2DB;

public sealed class GW2DbRepository<T>(IDbContextFactory<AppDbContext> _factory) : 
    IGW2DbRepository<T> where T : GW2APIBaseItem
{
    // Compiled ONCE (static): the LINQ expression tree is translated to SQL a single time,
    // every later call skips expression-tree processing and query-cache lookups.
    private static readonly Func<AppDbContext, long, Task<T?>> GetByIdCompiledAsync =
        EF.CompileAsyncQuery((AppDbContext db, long id) =>
            db.Set<T>()
              .AsNoTracking()
              .FirstOrDefault(p => p.Id == id));

    /// <summary>Highly optimized get-by-ID.</summary>
    public async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        await using AppDbContext db = await _factory.CreateDbContextAsync(ct);
        return await GetByIdCompiledAsync(db, id);
    }

    private static readonly Func<AppDbContext, long, T?> GetByIdCompiled =
        EF.CompileQuery((AppDbContext db, long id) =>
            db.Set<T>()
              .AsNoTracking()
              .FirstOrDefault(p => p.Id == id));

    public T? GetById(long id)
    {
        using AppDbContext db = _factory.CreateDbContext();
        return GetByIdCompiled(db, id);
    }
}
