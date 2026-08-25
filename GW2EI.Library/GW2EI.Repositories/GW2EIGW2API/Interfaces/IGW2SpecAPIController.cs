using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2SpecAPIController
{
    Task<GW2APISpec?> GetById(long ID);
    Task WriteAPISpecsToFile();
}
