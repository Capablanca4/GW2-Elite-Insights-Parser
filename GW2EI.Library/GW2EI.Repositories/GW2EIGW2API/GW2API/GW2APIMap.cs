namespace GW2EIGW2API.GW2API;

public class GW2APIMap : GW2APIBaseItem
{
    public string Name { get; set; }
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public int DefaultFloor { get; set; }
    public string Type { get; set; }
    [JsonColumn]
    public IReadOnlyList<int> Floors { get; set; }
    public int RegionId { get; set; }
    public string? RegionName { get; set; }
    public int ContinentId { get; set; }
    public string? ContinentName { get; set; }
    [JsonColumn]
    public IReadOnlyList<IReadOnlyList<int>> MapRect { get; set; }
    [JsonColumn]
    public IReadOnlyList<IReadOnlyList<int>> ContinentRect { get; set; }
}
