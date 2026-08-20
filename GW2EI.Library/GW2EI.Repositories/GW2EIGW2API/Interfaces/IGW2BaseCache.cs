using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2BaseCache<T> where T : GW2APIBaseItem
{
    Task WriteItemsToCache(IEnumerable<T> items);
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
