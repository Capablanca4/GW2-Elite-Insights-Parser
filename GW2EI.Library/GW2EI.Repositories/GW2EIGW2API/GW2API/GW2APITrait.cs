namespace GW2EIGW2API.GW2API;

public class GW2APITrait : GW2APIBaseItem
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }
    public int Specialization { get; set; }
    public int Tier { get; set; }
    public string Slot { get; set; }
    [JsonColumn]
    public IReadOnlyList<GW2APIFact>? Facts { get; set; }

    [JsonColumn]
    public IReadOnlyList<GW2APITraitedFact>? TraitedFacts { get; set; }
    [JsonColumn]
    public IReadOnlyList<GW2APISkill>? Skills { get; set; }
}
