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
    /// <param name="skillLocation"></param>
    /// <param name="specLocation"></param>
    /// <param name="traitLocation"></param>
    /// <param name="mapLocation"></param>
    public GW2APIController(string skillLocation, string specLocation, string traitLocation, string mapLocation)
    {
        skillAPIController = new GW2SkillAPIController(
            new GW2BaseCache<GW2APISkill>(skillLocation), 
            new GW2BaseAPI<GW2APISkill>("/v2/skills"));
        specAPIController = new GW2SpecAPIController(
            new GW2BaseCache<GW2APISpec>(specLocation), 
            new GW2BaseAPI<GW2APISpec>("/v2/specializations"));
        mapAPIController = new GW2MapAPIController(
            new GW2BaseCache<GW2APIMap>(mapLocation), 
            new GW2BaseAPI<GW2APIMap>("/v2/maps"));
        traitAPIController = new GW2TraitAPIController(
            new GW2BaseCache<GW2APITrait>(traitLocation), 
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

    public void WriteAPISkillsToFile()
    {
        skillAPIController.WriteAPISkillsToFile().RunSynchronously();
    }

    //----------------------------------------------------------------------------- SPECS
    /// <summary>
    /// Returns GW2APISpec item
    /// Warning: this method is not thread safe, 
    /// Make sure to initialize the cache before hand if you intend to call this method from different threads
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
   
    public static readonly string UNKNOWN_SPEC = "Unknown";
    public string GetSpec(uint prof, uint elite)
    {
        // Non player agents - Gadgets = GDG
        if (elite == 0xFFFFFFFF)
        {
            return (prof & 0xffff0000) == 0xffff0000 ? "GDG" : "NPC";
        }
        // Old way - Base Profession
        else if (elite == 0)
        {
            switch (prof)
            {
                case 1: return "Guardian";
                case 2: return "Warrior";
                case 3: return "Engineer";
                case 4: return "Ranger";
                case 5: return "Thief";
                case 6: return "Elementalist";
                case 7: return "Mesmer";
                case 8: return "Necromancer";
                case 9: return "Revenant";
                default: return UNKNOWN_SPEC;
            }
        }
        // Old way - Elite Specialization (HoT)
        else if (elite == 1)
        {
            switch (prof)
            {
                case 1: return "Dragonhunter";
                case 2: return "Berserker";
                case 3: return "Scrapper";
                case 4: return "Druid";
                case 5: return "Daredevil";
                case 6: return "Tempest";
                case 7: return "Chronomancer";
                case 8: return "Reaper";
                case 9: return "Herald";
                default: return UNKNOWN_SPEC;
            }
        }
        // Current way
        else
        {
            GW2APISpec? spec = specAPIController.GetById((int)elite).GetAwaiter().GetResult();
            if (spec is null)
            {
                return UNKNOWN_SPEC;
            }
            return spec.Elite ? spec.Name : spec.Profession;
        }
        throw new InvalidOperationException("Unexpected profession pattern in GetSpec");
    }

    public void WriteAPISpecsToFile()
    {
        specAPIController.WriteAPISpecsToFile().RunSynchronously();
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

    public void WriteAPIMapsToFile()
    {
        mapAPIController.WriteAPIMapsToFile().RunSynchronously();
    }

    //----------------------------------------------------------------------------- TRAITS
    /// <summary>
    /// Returns GW2APITrait item
    /// Warning: this method is not thread safe, 
    /// Make sure to initialize the cache before hand if you intend to call this method from different threads
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GW2APITrait GetAPITrait(long id)
    {
        return traitAPIController.GetById(id).GetAwaiter().GetResult();
    }

    public void WriteAPITraitsToFile()
    {
        traitAPIController.WriteAPITraitsToFile().RunSynchronously();
    }
}
