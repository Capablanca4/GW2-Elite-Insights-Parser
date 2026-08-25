using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

[assembly: CLSCompliant(false)]
namespace GW2EIGW2API;

public class GW2APIController(
    IGW2SkillAPIController skillAPIController, 
    IGW2SpecAPIController specAPIController, 
    IGW2TraitAPIController traitAPIController, 
    IGW2MapAPIController mapAPIController)
{
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
