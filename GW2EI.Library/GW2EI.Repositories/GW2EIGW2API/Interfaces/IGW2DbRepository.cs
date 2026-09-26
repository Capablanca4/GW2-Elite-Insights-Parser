using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2DbRepository<T> where T : GW2APIBaseItem
{
    Task<T> GetByIdAsync(long id, CancellationToken ct = default);
    T GetById(long id);
}
