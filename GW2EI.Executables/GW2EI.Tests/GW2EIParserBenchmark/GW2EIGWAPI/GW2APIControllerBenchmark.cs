using BenchmarkDotNet.Attributes;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;

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
        _apiController = new("./Content/SkillList.json", "./Content/SpecList.json", "./Content/TraitList.json", "./Content/MapList.json");
    }

    [Benchmark]
    public GW2APIController TestConstructorMemory()
    {
        GW2APIController controller = new("./Content/SkillList.json", "./Content/SpecList.json", "./Content/TraitList.json", "./Content/MapList.json");
        return controller;
    }

    [Benchmark]
    public GW2APISkill? GetAPISkill()
    {
        var skill = _apiController?.GetAPISkill(5555);
        return skill;
    }

    [Benchmark]
    public GW2APISpec? GetAPISpec()
    {
        var spec = _apiController?.GetAPISpec(1);
        return spec;
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
