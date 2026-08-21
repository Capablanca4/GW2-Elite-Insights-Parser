using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

internal class GW2SkillAPIController(IGW2BaseCache<GW2APISkill> _skillCache, IGW2BaseAPI<GW2APISkill> _skillAPI)
{
    public Task<GW2APISkill?> GetById(long ID)
    {
        return _skillCache.GetByIdAsync(ID);
    }

    public async Task WriteAPISkillsToFile()
    {
        IEnumerable<GW2APISkill> skills = await _skillAPI.GetGW2APIItems();
         _skillCache.WriteItemsToCache(skills.ToList());
    }
}
