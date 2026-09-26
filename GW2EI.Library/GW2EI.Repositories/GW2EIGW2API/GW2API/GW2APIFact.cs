using System.Text.Json.Serialization;

namespace GW2EIGW2API.GW2API;

[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(GW2APITraitedFact))]
public class GW2APIFact
{
    public string Text { get; set; }
    public string Icon { get; set; }
    public string Type { get; set; }
    public string Target { get; set; }
    public object Value { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public int ApplyCount { get; set; }
    public float Duration { get; set; }
    public string FieldType { get; set; }
    public string FinisherType { get; set; }
    public float Percent { get; set; }
    public int HitCount { get; set; }
    public float DmgMultiplier { get; set; }
    public int Distance { get; set; }
    [JsonColumn]
    public GW2APIFact Prefix { get; set; }
}

