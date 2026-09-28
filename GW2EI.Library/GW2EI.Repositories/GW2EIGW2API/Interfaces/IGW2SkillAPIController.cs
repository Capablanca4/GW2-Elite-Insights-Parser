using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2SkillAPIController
{
    GW2APISkill? GetById(long ID);
    Task<GW2APISkill?> GetByIdAsync(long ID, CancellationToken cancellationToken = default);
    Task WriteAPISkillsToFile(CancellationToken ctx = default);
}
