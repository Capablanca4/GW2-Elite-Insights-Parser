using BenchmarkDotNet.Attributes;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIParserBenchmark.GW2EIGWAPI;

[SimpleJob(
    warmupCount: 5,
    iterationCount: 10,
    launchCount: 1)]
[MemoryDiagnoser]
public class GW2APIControllerBenchmark
{
    private GW2APIController? _apiController;

    [Params(true, false)]
    public bool UseCachedRepository;

    [GlobalSetup]
    public void Setup()
    {
        _apiController = CreateController();
    }

    private GW2APIController CreateController()
    {
        IGW2HttpClient httpClient = new GW2HttpClient();

        IGW2DBRepository<T> CreateRepositories<T>(string fileIndex, string filePositions) where T : GW2APIBaseItem
        {
            if (UseCachedRepository)
            {
                return new CachedGW2DBRepository<T>(filePositions);
            }
            return new GW2DBRepository<T>(fileIndex, filePositions);
        }

        GW2SkillAPIController skillAPIController = new(
            CreateRepositories<GW2APISkill>("./Content/SkillList.index", "./Content/SkillList.json"),
            httpClient);
        GW2SpecAPIController specAPIController = new(
           CreateRepositories<GW2APISpec>("./Content/SpecList.index", "./Content/SpecList.json"),
           httpClient);
        GW2MapAPIController mapAPIController = new(
            CreateRepositories<GW2APIMap>("./Content/TraitList.index", "./Content/TraitList.json"),
            httpClient);
        GW2TraitAPIController traitAPIController = new(
            CreateRepositories<GW2APITrait>("./Content/MapList.index", "./Content/MapList.json"),
            httpClient);

        GW2APIController controller = new(skillAPIController, specAPIController, traitAPIController, mapAPIController);
        return controller;
    }

    [Benchmark]
    public GW2APIController TestConstructorMemory()
    {
        GW2APIController controller = CreateController();
        return controller;
    }

    [Benchmark]
    public GW2APISkill? GetAPISkill()
    {
        var skill = _apiController?.GetAPISkill(5555);
        return skill;
    }

    [Benchmark]
    public GW2APIMap? GetAPIMap()
    {
        var map = _apiController?.GetAPIMap(50);
        return map;
    }

    [Benchmark]
    public GW2APITrait? GetAPITrait()
    {
        var trait = _apiController?.GetAPITrait(888);
        return trait;
    }

    [Benchmark]
    public string? GetSpec()
    {
        var spec = _apiController?.GetSpec(2, 5);
        return spec;
    }
}
