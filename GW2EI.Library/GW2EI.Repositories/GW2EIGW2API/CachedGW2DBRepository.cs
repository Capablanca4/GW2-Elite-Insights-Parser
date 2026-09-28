using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public sealed class CachedGW2DBRepository<T> : IGW2DBRepository<T> where T : GW2APIBaseItem
{
    private readonly FrozenDictionary<long, T> _items;

    public CachedGW2DBRepository(string filePositions)
    {
        var fi = new FileInfo(filePositions);
        if (!fi.Exists || fi.Length == 0)
        {
            throw new FileNotFoundException($"File {filePositions} does not exist or is empty.");
        }

        using FileStream reader = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        JsonTypeInfo<List<T>> test = GW2JsonContext.ResolveTypeInfo<List<T>>();
        List<T> items = JsonSerializer.Deserialize(reader, test);
        _items = items.ToFrozenDictionary(i => i.Id);
    }

    public T? GetById(long id)
    {
        return _items.GetValueOrDefault(id);
    }

    public Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetById(id));
    }

    public void WriteItemsToCache(IList<T> items)
    {
        throw new NotImplementedException("Writing items to cache is not implemented.");
    }
}
