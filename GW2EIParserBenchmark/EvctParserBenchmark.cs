using BenchmarkDotNet.Attributes;
using GW2EIEvtcParser;
using GW2EIEvtcParser.ParserHelpers;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;

namespace GW2EIParserBenchmark;

[MemoryDiagnoser]
public class EvctParserBenchmark
{
    [Params("./TestFiles/20241207-011501.zevtc")]
    public string _testFile;
    public EvtcParser parser;
    ParserController parserController = new TestOperationController();

    [GlobalSetup]
    public void Setup()
    {
        GW2SkillAPIController skillAPIController = new(
            new GW2BaseCache<GW2APISkill>("./Content/SkillList.index", "./Content/SkillList.json"),
            new GW2BaseAPI<GW2APISkill>("/v2/skills"));
         GW2SpecAPIController specAPIController = new(
            new GW2BaseCache<GW2APISpec>("./Content/SpecList.index", "./Content/SpecList.json"),
            new GW2BaseAPI<GW2APISpec>("/v2/specializations"));
        GW2MapAPIController mapAPIController = new(
            new GW2BaseCache<GW2APIMap>("./Content/MapList.index", "./Content/MapList.json"),
            new GW2BaseAPI<GW2APIMap>("/v2/maps"));
        GW2TraitAPIController traitAPIController = new(
            new GW2BaseCache<GW2APITrait>("./Content/TraitList.index", "./Content/TraitList.json"),
            new GW2BaseAPI<GW2APITrait>("/v2/traits"));

        EvtcParserSettings parserSettings = new(0, 0);
        GW2APIController apiController = new(skillAPIController, specAPIController, traitAPIController, mapAPIController);
        parser = new EvtcParser(parserSettings, apiController);
    }
    
    [Benchmark]
    public ParsedEvtcLog? ParseLog()
    {
        FileInfo fileInfo = new(_testFile);
        ParsedEvtcLog? test = parser.ParseLog(parserController, fileInfo, out ParsingFailureReason? parsingFailureReasure);
        return test;
    }
}
