using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public sealed class GW2SpecAPIController(
    IGW2BaseCache<GW2APISpec> _specCache, 
    IGW2HttpClient _client) : 
    IGW2SpecAPIController
{
    public Task<GW2APISpec?> GetById(long ID)
    {
        return _specCache.GetByIdAsync(ID);
    }

    public async Task WriteAPISpecsToFile(CancellationToken ctx = default)
    {
        IEnumerable<GW2APISpec> specs = await _client.GetGW2APIItems<GW2APISpec>("/v2/specs", ctx);
        _specCache.WriteItemsToCache(specs.ToList());
    }
}
