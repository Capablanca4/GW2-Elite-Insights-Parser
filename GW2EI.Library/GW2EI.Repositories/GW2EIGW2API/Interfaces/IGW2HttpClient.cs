using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2HttpClient
{
    Task<IEnumerable<T>> GetGW2APIItems<T>(string apiPath, CancellationToken ct = default) where T : GW2APIBaseItem;
    Task<T?> GetGW2APIItem<T>(string apiPath, CancellationToken ct = default) where T : GW2APIBaseItem;
}
