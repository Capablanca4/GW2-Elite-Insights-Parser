using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public sealed class GW2TraitAPIController(
    IGW2BaseCache<GW2APITrait> _traitCache, 
    IGW2HttpClient _client) : 
    IGW2TraitAPIController
{
    public Task<GW2APITrait?> GetById(long ID)
    {
        return _traitCache.GetByIdAsync(ID);
    }

    public async Task WriteAPITraitsToFile(CancellationToken ctx = default)
    {
        IEnumerable<GW2APITrait> traits = await _client.GetGW2APIItems<GW2APITrait>("/v2/traits", ctx);
        _traitCache.WriteItemsToCache(traits.ToList());
    }
}
