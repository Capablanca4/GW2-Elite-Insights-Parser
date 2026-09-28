using GW2EIGW2API.Models;

namespace GW2EIGW2API.Interfaces;

public interface IGW2SkillAPIController
{
    Task<GW2APISkill?> GetById(long ID);
    Task WriteAPISkillsToFile(CancellationToken ctx = default);
}
