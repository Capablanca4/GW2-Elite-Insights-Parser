using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2BaseAPI<T> where T : GW2APIBaseItem
{
    Task<IEnumerable<T>> GetGW2APIItems();
}
