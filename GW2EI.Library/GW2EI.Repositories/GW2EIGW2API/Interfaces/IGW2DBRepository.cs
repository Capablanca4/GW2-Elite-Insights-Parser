using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2DBRepository<T> where T : GW2APIBaseItem
{
    T GetById(long id);
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    void WriteItemsToCache(IList<T> items);
}
