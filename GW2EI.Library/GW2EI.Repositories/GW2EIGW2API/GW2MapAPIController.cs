using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public sealed class GW2MapAPIController(
    IGW2BaseCache<GW2APIMap> _mapCache,
    IGW2BaseAPI<GW2APIMap> _mapAPI) : 
    IGW2MapAPIController
{
    public Task<GW2APIMap?> GetById(long ID)
    {
        return _mapCache.GetByIdAsync(ID);
    }

    public async Task WriteAPIMapsToFile()
    {
        IEnumerable<GW2APIMap> maps = await _mapAPI.GetGW2APIItems();
        _mapCache.WriteItemsToCache(maps.ToList());
    }
}
