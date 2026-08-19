using BenchmarkDotNet.Attributes;
using GW2EIEvtcParser;
using GW2EIEvtcParser.ParserHelpers;
using GW2EIGW2API;

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
        EvtcParserSettings parserSettings = new(0, 0);
        GW2APIController apiController = new("./Content/SkillList.json", "./Content/SpecList.json", "./Content/TraitList.json", "./Content/MapList.json");
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
