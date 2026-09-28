using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GW2EIGW2API.GW2API;

// Same options as the previous reflection-based SerializerSettings.
// (Like before, the default encoder HTML-escapes characters when writing.)
[JsonSourceGenerationOptions(
    WriteIndented = false,
    IncludeFields = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GW2APISkill))]
[JsonSerializable(typeof(GW2APISpec))]
[JsonSerializable(typeof(GW2APIMap))]
[JsonSerializable(typeof(GW2APITrait))]
[JsonSerializable(typeof(List<GW2APISkill>))]
[JsonSerializable(typeof(List<GW2APISpec>))]
[JsonSerializable(typeof(List<GW2APIMap>))]
[JsonSerializable(typeof(List<GW2APITrait>))]
internal partial class GW2JsonContext : JsonSerializerContext 
{ 
    public static JsonTypeInfo<T> ResolveTypeInfo<T>()
    {
        return Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
            ?? throw new InvalidOperationException(
                $"{typeof(T).Name} is not registered in {nameof(GW2JsonContext)}. Add a [JsonSerializable(typeof({typeof(T).Name}))] attribute.");
    }
}

[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(GW2APISkill))]
[JsonDerivedType(typeof(GW2APISpec))]
[JsonDerivedType(typeof(GW2APITrait))]
[JsonDerivedType(typeof(GW2APIMap))]
public abstract class GW2APIBaseItem
{
    public long Id;
}
