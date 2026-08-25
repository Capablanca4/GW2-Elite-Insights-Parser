using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2TraitAPIController
{
    Task<GW2APITrait?> GetById(long ID);
    Task WriteAPITraitsToFile();
}
