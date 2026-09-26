using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public sealed class GW2MapAPIController(
    IGW2BaseCache<GW2APIMap> _mapCache,
    IGW2HttpClient _client) : 
    IGW2MapAPIController
{
    public Task<GW2APIMap?> GetById(long ID)
    {
        return _mapCache.GetByIdAsync(ID);
    }

    public async Task WriteAPIMapsToFile(CancellationToken ctx = default)
    {
        IEnumerable<GW2APIMap> maps = await _client.GetGW2APIItems<GW2APIMap>("/v2/maps", ctx);
        _mapCache.WriteItemsToCache(maps.ToList());
    }
}
