using BenchmarkDotNet.Attributes;
using GW2EIEvtcParser;
using GW2EIEvtcParser.ParserHelpers;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.GW2DB;
using GW2EIGW2API.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

        bool useMemory = true;
        string dbFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Content/GW2.db";
        string inMemoryDbPath = "file:memdb1?mode=memory&cache=shared";
        string dbPath = useMemory ? inMemoryDbPath : dbFilePath;
        SqliteConnection sharedConnection = new($"Data Source={dbPath}");
        sharedConnection.Open();

        if (useMemory)
        {
            using SqliteConnection physicalConnection = new($"Data Source={dbFilePath}");
            physicalConnection.Open();
            physicalConnection.BackupDatabase(sharedConnection);
        }

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(sharedConnection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .AddInterceptors(new SqlitePragmaInterceptor())
            .Options;

        // creates the database if it doesn't exist
        PooledDbContextFactory<AppDbContext> factory = new(options, poolSize: 128);
        factory.CreateDbContext().Database.EnsureCreated();

        GW2DbRepository<GW2APISkill> skillAPIController = new(factory);
        GW2DbRepository<GW2APISpec> specAPIController = new(factory);
        GW2DbRepository<GW2APIMap> mapAPIController = new(factory);
        GW2DbRepository<GW2APITrait> traitAPIController = new(factory);
        GW2APIController controller = new(skillAPIController, specAPIController, traitAPIController, mapAPIController);

        EvtcParserSettings parserSettings = new(0, 0);
        parser = new EvtcParser(parserSettings, controller);
    }
    
    [Benchmark]
    public ParsedEvtcLog? ParseLog()
    {
        FileInfo fileInfo = new(_filePath);
        ParsedEvtcLog? test = parser.ParseLog(parserController, fileInfo, out ParsingFailureReason? parsingFailureReasure);
        return test;
    }
}
