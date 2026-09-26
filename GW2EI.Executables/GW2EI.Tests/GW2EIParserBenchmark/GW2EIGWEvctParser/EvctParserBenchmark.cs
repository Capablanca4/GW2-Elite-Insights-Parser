using BenchmarkDotNet.Attributes;
using GW2EIEvtcParser;
using GW2EIEvtcParser.ParserHelpers;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIParserBenchmark;

[MemoryDiagnoser]
public class EvctParserBenchmark
{
    [ParamsSource(nameof(Files))]
    public string _filePath { get; set; } = null!;

    public static IEnumerable<string> Files()
    {
        string testFilesPath = Path.Combine(AppContext.BaseDirectory, "TestFiles");
        if (Directory.Exists(testFilesPath))
        {
            return Directory.EnumerateFiles(testFilesPath, "*.zevtc", SearchOption.TopDirectoryOnly);
        }

        throw new Exception($"No files are present in {testFilesPath}");
    }

    public EvtcParser parser;
    ParserController parserController = new TestOperationController();

    [GlobalSetup]
    public void Setup()
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

        EvtcParserSettings parserSettings = new(0, 0);
        GW2APIController apiController = new(skillAPIController, specAPIController, traitAPIController, mapAPIController);
        parser = new EvtcParser(parserSettings, apiController);
    }
    
    [Benchmark]
    public ParsedEvtcLog? ParseLog()
    {
        FileInfo fileInfo = new(_filePath);
        ParsedEvtcLog? test = parser.ParseLog(parserController, fileInfo, out ParsingFailureReason? parsingFailureReasure);
        return test;
    }
}
