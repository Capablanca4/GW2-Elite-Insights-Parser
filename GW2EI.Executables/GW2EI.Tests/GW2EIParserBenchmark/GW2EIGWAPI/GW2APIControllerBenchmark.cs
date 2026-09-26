using BenchmarkDotNet.Attributes;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.GW2DB;
using GW2EIGW2API.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
            physicalConnection.Close();
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
