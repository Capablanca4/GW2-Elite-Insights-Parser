using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

internal class GW2TraitAPIController(IGW2BaseCache<GW2APITrait> _traitCache, IGW2BaseAPI<GW2APITrait> _traitAPI)
{
    public Task<GW2APITrait?> GetById(long ID)
    {
        return _traitCache.GetByIdAsync(ID);
    }

    public async Task WriteAPITraitsToFile()
    {
        IEnumerable<GW2APITrait> traits = await _traitAPI.GetGW2APIItems();
        _traitCache.WriteItemsToCache(traits.ToList());
    }
}
