using GW2EIGW2API.Models;

namespace GW2EIGW2API.Interfaces;

public interface IGW2SpecAPIController
{
    Task<GW2APISpec?> GetById(long ID);
    Task WriteAPISpecsToFile(CancellationToken ctx = default);
}
