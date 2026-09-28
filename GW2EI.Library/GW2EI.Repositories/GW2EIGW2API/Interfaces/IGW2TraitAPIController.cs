using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2TraitAPIController
{
    GW2APITrait? GetById(long ID);
    Task<GW2APITrait?> GetByIdAsync(long ID, CancellationToken cancellationToken = default);
    Task WriteAPITraitsToFile(CancellationToken ctx = default);
}
