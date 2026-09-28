using BenchmarkDotNet.Attributes;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.GW2DB;
using GW2EIGW2API.Interfaces;
using GW2EIGW2API.Models;
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

    [Params(true, false)]
    public bool HighPerf;

    private GW2APIController CreateController()
    {
        IGW2HttpClient httpClient = new GW2HttpClient();

        string dbFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Content/GW2.db";
        SqliteConnection sharedConnection = new($"Data Source={dbFilePath}");
        sharedConnection.Open();

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(sharedConnection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .AddInterceptors(new SqlitePragmaInterceptor())
            .Options;

        // creates the database if it doesn't exist
        PooledDbContextFactory<AppDbContext> factory = new(options, poolSize: 128);
        using (AppDbContext db = factory.CreateDbContext())
        {
            db.Database.EnsureCreated();   // creates the database if it doesn't exist
        }

        // The only place where the mode picks an implementation
        IGW2DbRepository<T> CreateRepository<T>() where T : GW2APIBaseItem
        {
            return HighPerf ? new CachedGW2Repository<T>(factory) : new GW2DbRepository<T>(factory);
        }

        long before = GC.GetTotalMemory(forceFullCollection: true);
        GW2APIController controller = new(
            CreateRepository<GW2APISkill>(),
            CreateRepository<GW2APISpec>(),
            CreateRepository<GW2APITrait>(),
            CreateRepository<GW2APIMap>());
        long after = GC.GetTotalMemory(forceFullCollection: true);
        Console.WriteLine($"Retained: {after - before:N0} B");

        return controller;
    }

    [GlobalSetup]
    public void Setup()
    {
        _apiController = CreateController();
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
