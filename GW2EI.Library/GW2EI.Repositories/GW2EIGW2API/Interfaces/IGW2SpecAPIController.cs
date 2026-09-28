using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2SpecAPIController
{
    GW2APISpec? GetById(long ID);
    Task<GW2APISpec?> GetByIdAsync(long ID, CancellationToken cancellationToken = default);
    Task WriteAPISpecsToFile(CancellationToken ctx = default);
}
