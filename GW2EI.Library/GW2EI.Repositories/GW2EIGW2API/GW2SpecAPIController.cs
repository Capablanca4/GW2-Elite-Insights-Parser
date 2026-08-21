using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

internal class GW2SpecAPIController(IGW2BaseCache<GW2APISpec> _specCache, IGW2BaseAPI<GW2APISpec> _specAPI)
{
    public Task<GW2APISpec?> GetById(long ID)
    {
        return _specCache.GetByIdAsync(ID);
    }

    public async Task WriteAPISpecsToFile()
    {
        IEnumerable<GW2APISpec> specs = await _specAPI.GetGW2APIItems();
         _specCache.WriteItemsToCache(specs.ToList());
    }
}
