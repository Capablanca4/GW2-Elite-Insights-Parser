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
    public static readonly string UNKNOWN_SPEC = "Unknown";
    public string GetSpec(uint prof, uint elite)
    {
        return (elite, prof) switch
        {
            // Non player agents - Gadgets = GDG
            (0xFFFFFFFF, _) => (prof & 0xffff0000) == 0xffff0000 ? "GDG" : "NPC",
            // Old way - Base Profession
            (0, 1) => "Guardian",
            (0, 2) => "Warrior",
            (0, 3) => "Engineer",
            (0, 4) => "Ranger",
            (0, 5) => "Thief",
            (0, 6) => "Elementalist",
            (0, 7) => "Mesmer",
            (0, 8) => "Necromancer",
            (0, 9) => "Revenant",
            // Old way - Elite Specialization (HoT)
            (1, 1) => "Dragonhunter",
            (1, 2) => "Berserker",
            (1, 3) => "Scrapper",
            (1, 4) => "Druid",
            (1, 5) => "Daredevil",
            (1, 6) => "Tempest",
            (1, 7) => "Chronomancer",
            (1, 8) => "Reaper",
            (1, 9) => "Herald",
            // New way 
            _ => GetSpecNew(prof)
        };
    }

    private string GetSpecNew(uint id)
    {
        GW2APISpec? spec = specAPIController.GetById(id).GetAwaiter().GetResult();
        if (spec is null)
        {
            return UNKNOWN_SPEC;
        }
        return spec.Elite ? spec.Name : spec.Profession;
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
