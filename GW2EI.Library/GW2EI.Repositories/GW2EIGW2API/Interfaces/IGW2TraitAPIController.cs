using GW2EIGW2API.Models;

namespace GW2EIGW2API.Interfaces;

public interface IGW2TraitAPIController
{
    Task<GW2APITrait?> GetById(long ID);
    Task WriteAPITraitsToFile(CancellationToken ctx = default);
}
