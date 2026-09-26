using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GW2EIGW2API.GW2API;

[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(GW2APISkill))]
[JsonDerivedType(typeof(GW2APISpec))]
[JsonDerivedType(typeof(GW2APITrait))]
[JsonDerivedType(typeof(GW2APIMap))]
[JsonDerivedType(typeof(GW2APiBuild))]
public abstract class GW2APIBaseItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
}
