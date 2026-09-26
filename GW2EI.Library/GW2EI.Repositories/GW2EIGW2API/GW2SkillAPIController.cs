using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public sealed class GW2SkillAPIController(
    IGW2BaseCache<GW2APISkill> _skillCache, 
    IGW2HttpClient _client) : 
    IGW2SkillAPIController
{
    public Task<GW2APISkill?> GetById(long ID)
    {
        return _skillCache.GetByIdAsync(ID);
    }

    public async Task WriteAPISkillsToFile(CancellationToken ctx = default)
    {
        IEnumerable<GW2APISkill> skills = await _client.GetGW2APIItems<GW2APISkill>("/v2/skills", ctx);
        _skillCache.WriteItemsToCache(skills.ToList());
    }
}
