using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public sealed class GW2SpecAPIController(
    IGW2DBRepository<GW2APISpec> _specCache, 
    IGW2HttpClient _client) : 
    IGW2SpecAPIController
{
    public GW2APISpec? GetById(long ID)
    {
        return _specCache.GetById(ID);
    }

    public Task<GW2APISpec?> GetByIdAsync(long ID, CancellationToken cancellationToken = default)
    {
        return _specCache.GetByIdAsync(ID, cancellationToken);
    }

    public async Task WriteAPISpecsToFile(CancellationToken ctx = default)
    {
        IEnumerable<GW2APISpec> specs = await _client.GetGW2APIItems<GW2APISpec>("/v2/specs", ctx);
        _specCache.WriteItemsToCache(specs.ToList());
    }
}
