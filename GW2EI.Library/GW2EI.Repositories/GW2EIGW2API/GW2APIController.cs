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
    #region SKILLS

    public GW2APISkill? GetAPISkill(long id)
    {
        return skillAPIController.GetById(id);
    }

    public Task<GW2APISkill?> GetAPISkillAsync(long id, CancellationToken cancellationToken = default)
    {
        return skillAPIController.GetByIdAsync(id, cancellationToken);
    }

    public Task WriteAPISkillsToFile()
    {
        return skillAPIController.WriteAPISkillsToFile();
    }

    #endregion

    #region SPECS

    public static readonly string UNKNOWN_SPEC = "Unknown";

    public string GetSpec(uint prof, uint elite)
    {
        return GetSpecOld(prof, elite) ?? GetSpecNew(elite);
    }

    public async Task<string> GetSpecAsync(uint prof, uint elite, CancellationToken cancellationToken = default)
    {
        return GetSpecOld(prof, elite) ?? await GetSpecNewAsync(elite, cancellationToken);
    }

    private string? GetSpecOld(uint prof, uint elite)
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
            _ => null
        };
    }

    private string GetSpecNew(uint id)
    {
        GW2APISpec? spec = specAPIController.GetById(id);
        if (spec is null)
        {
            return UNKNOWN_SPEC;
        }
        return spec.Elite ? spec.Name : spec.Profession;
    }

    private async Task<string> GetSpecNewAsync(uint id, CancellationToken cancellationToken = default)
    {
        GW2APISpec? spec = await specAPIController.GetByIdAsync(id, cancellationToken);
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

    #endregion

    #region MAPS

    public GW2APIMap? GetAPIMap(int id)
    {
        return mapAPIController.GetById(id);
    }

    public Task<GW2APIMap?> GetAPIMapAsync(int id, CancellationToken cancellationToken = default)
    {
        return mapAPIController.GetByIdAsync(id, cancellationToken);
    }

    public Task WriteAPIMapsToFile()
    {
        return mapAPIController.WriteAPIMapsToFile();
    }

    #endregion

    #region TRAITS

    public GW2APITrait? GetAPITrait(long id)
    {
        return traitAPIController.GetById(id);
    }

    public Task<GW2APITrait?> GetAPITraitAsync(long id, CancellationToken cancellationToken = default)
    {
        return traitAPIController.GetByIdAsync(id, cancellationToken);
    }

    public Task WriteAPITraitsToFile()
    {
        return traitAPIController.WriteAPITraitsToFile();
    }

    #endregion
}
