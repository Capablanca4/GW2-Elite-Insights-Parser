using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2MapAPIController
{
    Task<GW2APIMap?> GetById(long ID);
    Task WriteAPIMapsToFile();
}
