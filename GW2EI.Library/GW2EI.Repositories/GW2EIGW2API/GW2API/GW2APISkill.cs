namespace GW2EIGW2API.GW2API;

public class GW2APISkill : GW2APIBaseItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Icon { get; set; }
    public string ChatLink { get; set; }
    public string? Type { get; set; }
    public string? WeaponType { get; set; }
    [JsonColumn]
    public IReadOnlyList<string>? Professions { get; set; }
    [JsonColumn]
    public IReadOnlyList<string> Flags { get; set; }
    public string? Slot { get; set; }
    [JsonColumn]
    public IReadOnlyList<GW2APIFact>? Facts { get; set; }
    [JsonColumn]
    public IReadOnlyList<GW2APITraitedFact>? TraitedFacts { get; set; }
    [JsonColumn]
    public IReadOnlyList<string>? Categories { get; set; }
    public string? Attunement { get; set; }
    public int Cost { get; set; }
    public string? DualWield { get; set; }
    public int FlipSkill { get; set; }
    public int Initiative { get; set; }
    public int NextChain { get; set; }
    public int PrevChain { get; set; }
    [JsonColumn]
    public IReadOnlyList<long>? TransformSkills { get; set; }
    [JsonColumn]    
    public IReadOnlyList<long>? BundleSkills { get; set; }

    public int ToolbeltSkill { get; set; }
}
