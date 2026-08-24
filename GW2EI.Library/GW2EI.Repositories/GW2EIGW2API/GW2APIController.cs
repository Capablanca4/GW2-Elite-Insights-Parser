using GW2EIGW2API.GW2API;

[assembly: CLSCompliant(false)]
namespace GW2EIGW2API;

public class GW2APIController
{
    private readonly GW2SkillAPIController skillAPIController;
    private readonly GW2SpecAPIController specAPIController;
    private readonly GW2TraitAPIController traitAPIController;
    private readonly GW2MapAPIController mapAPIController;

    /// <summary>
    /// API Cache init with a cache file locations, 
    /// If the files are present, the content will be used to initialize the API caches
    /// Otherwise the caches will be built from GW2 API calls
    /// </summary>
    /// <param name="skillFolder"></param>
    /// <param name="specFolder"></param>
    /// <param name="traitFolder"></param>
    /// <param name="mapFolder"></param>
    public GW2APIController(string skillFolder, string specFolder, string traitFolder, string mapFolder)
    {
        skillAPIController = new GW2SkillAPIController(
            new GW2BaseCache<GW2APISkill>(Path.Combine(skillFolder, "SkillList.index"), Path.Combine(skillFolder, "SkillList.json")),
            new GW2BaseAPI<GW2APISkill>("/v2/skills"));
        specAPIController = new GW2SpecAPIController(
            new GW2BaseCache<GW2APISpec>(Path.Combine(specFolder, "SpecList.index"), Path.Combine(specFolder, "SpecList.json")),
            new GW2BaseAPI<GW2APISpec>("/v2/specializations"));
        mapAPIController = new GW2MapAPIController(
            new GW2BaseCache<GW2APIMap>(Path.Combine(traitFolder, "MapList.index"), Path.Combine(traitFolder, "MapList.json")), 
            new GW2BaseAPI<GW2APIMap>("/v2/maps"));
        traitAPIController = new GW2TraitAPIController(
            new GW2BaseCache<GW2APITrait>(Path.Combine(mapFolder, "TraitList.index"), Path.Combine(mapFolder, "TraitList.json")),
            new GW2BaseAPI<GW2APITrait>("/v2/traits"));
    }

    //----------------------------------------------------------------------------- SKILLS
    /// <summary>
    /// Returns GW2APISkill item
    /// Warning: this method is not thread safe, 
    /// Make sure to initialize the cache before hand if you intend to call this method from different threads
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GW2APISkill? GetAPISkill(long id)
    {
        return skillAPIController.GetById(id).GetAwaiter().GetResult();
    }

    public Task WriteAPISkillsToFile()
    {
        return skillAPIController.WriteAPISkillsToFile();
    }

    //----------------------------------------------------------------------------- SPECS
    /// <summary>
    /// Returns GW2APISpec item
    /// Warning: this method is not thread safe, 
    /// Make sure to initialize the cache before hand if you intend to call this method from different threads
    /// </summary>
    public Spec GetSpec(uint prof, uint elite)
    {
        return (elite, prof) switch
        {
            // Non player agents - Gadgets = GDG
            (0xFFFFFFFF, _) => (prof & 0xffff0000) == 0xffff0000 ? Spec.Gadget : Spec.NPC,
            // Old way - Base Profession
            (0, 1) => Spec.Guardian,
            (0, 2) => Spec.Warrior,
            (0, 3) => Spec.Engineer,
            (0, 4) => Spec.Ranger,
            (0, 5) => Spec.Thief,
            (0, 6) => Spec.Elementalist,
            (0, 7) => Spec.Mesmer,
            (0, 8) => Spec.Necromancer,
            (0, 9) => Spec.Revenant,
            // Old way - Elite Specialization (HoT)
            (1, 1) => Spec.Dragonhunter,
            (1, 2) => Spec.Berserker,
            (1, 3) => Spec.Scrapper,
            (1, 4) => Spec.Druid,
            (1, 5) => Spec.Daredevil,
            (1, 6) => Spec.Tempest,
            (1, 7) => Spec.Chronomancer,
            (1, 8) => Spec.Reaper,
            (1, 9) => Spec.Herald,
            // New way 
            _ => GetSpecNew(prof)
        };
    }

    private Spec GetSpecNew(uint id)
    {
        GW2APISpec? apiSpec = specAPIController.GetById(id).GetAwaiter().GetResult();
        if (apiSpec is null)
        {
            return Spec.Unknown;
        }

        Dictionary<string, Spec> specs = new()
        {
            { "NPC", Spec.NPC },
            { "GDG", Spec.Gadget },
            //
            { "Galeshot", Spec.Galeshot },
            { "Untamed", Spec.Untamed },
            { "Druid", Spec.Druid },
            { "Soulbeast", Spec.Soulbeast },
            { "Ranger", Spec.Ranger },
            //
            { "Amalgam", Spec.Amalgam },
            { "Scrapper", Spec.Scrapper },
            { "Holosmith", Spec.Holosmith },
            { "Mechanist", Spec.Mechanist },
            { "Engineer", Spec.Engineer },
            //
            { "Antiquary", Spec.Antiquary },
            { "Specter", Spec.Specter },
            { "Daredevil", Spec.Daredevil },
            { "Deadeye", Spec.Deadeye },
            { "Thief", Spec.Thief },
            //
            { "Evoker", Spec.Evoker },
            { "Catalyst", Spec.Catalyst },
            { "Weaver", Spec.Weaver },
            { "Tempest", Spec.Tempest },
            { "Elementalist", Spec.Elementalist },
            //
            { "Troubadour", Spec.Troubadour },
            { "Virtuoso", Spec.Virtuoso },
            { "Mirage", Spec.Mirage },
            { "Chronomancer", Spec.Chronomancer },
            { "Mesmer", Spec.Mesmer },
            //
            { "Ritualist", Spec.Ritualist },
            { "Harbinger", Spec.Harbinger },
            { "Scourge", Spec.Scourge },
            { "Reaper", Spec.Reaper },
            { "Necromancer", Spec.Necromancer },
            //
            { "Paragon", Spec.Paragon },
            { "Bladesworn", Spec.Bladesworn },
            { "Spellbreaker", Spec.Spellbreaker },
            { "Berserker", Spec.Berserker },
            { "Warrior", Spec.Warrior },
            //
            { "Luminary", Spec.Luminary },
            { "Willbender", Spec.Willbender },
            { "Firebrand", Spec.Firebrand },
            { "Dragonhunter", Spec.Dragonhunter },
            { "Guardian", Spec.Guardian },
            //
            { "Conduit", Spec.Conduit },
            { "Vindicator", Spec.Vindicator },
            { "Renegade", Spec.Renegade },
            { "Herald", Spec.Herald },
            { "Revenant", Spec.Revenant },
            //
            { "", Spec.Unknown },
        };

        string name = apiSpec.Elite ? apiSpec.Name : apiSpec.Profession;
        if(specs.TryGetValue(name, out Spec spec))
        {
            return spec;
        }
        return Spec.Unknown;
    }


    public Task WriteAPISpecsToFile()
    {
        return specAPIController.WriteAPISpecsToFile();
    }

    //----------------------------------------------------------------------------- MAPS
    /// <summary>
    /// Returns GW2APIMap item
    /// Warning: this method is not thread safe, 
    /// Make sure to initialize the cache before hand if you intend to call this method from different threads
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GW2APIMap? GetAPIMap(int id)
    {
        return mapAPIController.GetById(id).GetAwaiter().GetResult();
    }

    public Task WriteAPIMapsToFile()
    {
        return mapAPIController.WriteAPIMapsToFile();
    }

    //----------------------------------------------------------------------------- TRAITS
    /// <summary>
    /// Returns GW2APITrait item
    /// Warning: this method is not thread safe, 
    /// Make sure to initialize the cache before hand if you intend to call this method from different threads
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GW2APITrait? GetAPITrait(long id)
    {
        return traitAPIController.GetById(id).GetAwaiter().GetResult();
    }

    public Task WriteAPITraitsToFile()
    {
        return traitAPIController.WriteAPITraitsToFile();
    }
}
