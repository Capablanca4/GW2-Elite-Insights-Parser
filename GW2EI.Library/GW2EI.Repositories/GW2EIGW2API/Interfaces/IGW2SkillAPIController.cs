using GW2EIGW2API.GW2API;

namespace GW2EIGW2API.Interfaces;

public interface IGW2SkillAPIController
{
    Task<GW2APISkill?> GetById(long ID);
    Task WriteAPISkillsToFile();
}
