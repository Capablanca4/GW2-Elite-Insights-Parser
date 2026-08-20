using System.Diagnostics.CodeAnalysis;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;

namespace GW2EIEvtcParser.ParsedData;

public class SkillData
{
    // Fields
    private readonly Dictionary<long, SkillItem?> _skills = [];
    private readonly GW2APIController _apiController;
    public readonly long DodgeID;
    public readonly long GenericBreakbarID;
    // Public Methods

    internal SkillData(GW2APIController apiController, EvtcVersionEvent evtcVersion, IEnumerable<SkillItem> skills)
    {
        _apiController = apiController;
        (DodgeID, GenericBreakbarID) = SkillItem.GetArcDPSCustomIDs(evtcVersion);
        _skills = skills.GroupBy(x => x.ID).ToDictionary(x => x.Key, x => x.FirstOrDefault());
    }

    public SkillItem? Get(long ID)
    {
        if (_skills.TryGetValue(ID, out var value))
        {
            return value;
        }
        GW2APISkill? skill = _apiController.GetAPISkill(ID);
        SkillItem skillItem = new(ID, SkillItem.DefaultName, skill);
        _skills.TryAdd(ID, skillItem);
        return skillItem;
    }

    
    internal bool TryGet(long ID, [NotNullWhen(true)] out SkillItem? skillItem)
    {
        return _skills.TryGetValue(ID, out skillItem);
    }

    internal HashSet<long> NotAccurate = [];

    public bool IsNotAccurate(long ID)
    {
        return NotAccurate.Contains(ID);
    }

    internal HashSet<long> GearProc = [];
    public bool IsGearProc(long ID)
    {
        return GearProc.Contains(ID);
    }

    internal HashSet<long> TraitProc = [];
    public bool IsTraitProc(long ID)
    {
        return TraitProc.Contains(ID);
    }

    internal HashSet<long> UnconditionalProc = [];
    public bool IsUnconditionalProc(long ID)
    {
        return UnconditionalProc.Contains(ID);
    }

    internal void CombineWithSkillInfo(Dictionary<long, SkillInfoEvent> skillInfoEvents)
    {
        foreach (KeyValuePair<long, SkillItem> pair in _skills)
        {
            if (skillInfoEvents.TryGetValue(pair.Key, out var skillInfoEvent))
            {
                pair.Value.AttachSkillInfoEvent(skillInfoEvent);
            }
        }
    }

}
