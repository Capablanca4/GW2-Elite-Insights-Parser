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

    [GlobalSetup]
    public void Setup()
    {
        _apiController = CreateController();
    }

    private GW2APIController CreateController()
    {
        IGW2HttpClient httpClient = new GW2HttpClient();

        GW2SkillAPIController skillAPIController = new(
            new GW2BaseCache<GW2APISkill>("./Content/SkillList.index", "./Content/SkillList.json"),
            httpClient);
        GW2SpecAPIController specAPIController = new(
           new GW2BaseCache<GW2APISpec>("./Content/SpecList.index", "./Content/SpecList.json"),
           httpClient);
        GW2MapAPIController mapAPIController = new(
            new GW2BaseCache<GW2APIMap>("./Content/MapList.index", "./Content/MapList.json"),
            httpClient);
        GW2TraitAPIController traitAPIController = new(
            new GW2BaseCache<GW2APITrait>("./Content/TraitList.index", "./Content/TraitList.json"),
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
        var skill = _apiController?.GetAPISkill(0);
        return skill;
    }

    [Benchmark]
    public GW2APISpec? GetAPISpec()
    {
        throw new NotImplementedException("GetAPISpec is not implemented yet.");
    }

    [Benchmark]
    public GW2APIMap? GetAPIMap()
    {
        var map = _apiController?.GetAPIMap(0);
        return map;
    }

    [Benchmark]
    public GW2APITrait? GetAPITrait()
    {
        var trait = _apiController?.GetAPITrait(0);
        return trait;
    }

    [Benchmark]
    public string? GetSpec()
    {
        var spec = _apiController?.GetSpec(0, 0);
        return spec;
    }
}
