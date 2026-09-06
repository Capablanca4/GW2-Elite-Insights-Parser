using BenchmarkDotNet.Attributes;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;

namespace GW2EIParserBenchmark;

[MemoryDiagnoser]
public class GW2APIControllerBenchmark
{
    GW2APIController _apiController;

    [GlobalSetup]
    public void Setup()
    {
        _apiController = new("./Content/SkillList.json", "./Content/SpecList.json", "./Content/TraitList.json", "./Content/MapList.json");
    }

    [Benchmark]
    public object TestConstructorMemory()
    {
        GW2APIController controller = new("./Content/SkillList.json", "./Content/SpecList.json", "./Content/TraitList.json", "./Content/MapList.json");
        return controller;
    }

    [Benchmark]
    public object GetAPISkill()
    {
        GW2APISkill skill = _apiController.GetAPISkill(0);
        return skill;
    }

    [Benchmark]
    public object GetAPISpec()
    {
        GW2APISpec spec = _apiController.GetAPISpec(0);
        return spec;
    }

    [Benchmark]
    public object GetAPIMap()
    {
        GW2APIMap map = _apiController.GetAPIMap(0);
        return map;
    }

    [Benchmark]
    public object GetAPITrait()
    {
        GW2APITrait trait = _apiController.GetAPITrait(0);
        return trait;
    }

    [Benchmark]
    public string GetSpec()
    {
        string spec = _apiController.GetSpec(0, 0);
        return spec;
    }
}
