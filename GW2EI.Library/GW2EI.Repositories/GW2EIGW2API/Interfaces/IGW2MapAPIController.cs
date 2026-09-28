using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2MapAPIController
{
    GW2APIMap? GetById(long ID);
    Task<GW2APIMap?> GetByIdAsync(long ID, CancellationToken cancellationToken = default);
    Task WriteAPIMapsToFile(CancellationToken ctx = default);
}
