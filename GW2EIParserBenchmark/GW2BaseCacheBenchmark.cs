using BenchmarkDotNet.Attributes;
using GW2EIGW2API;
using GW2EIGW2API.GW2API;

namespace GW2EIParserBenchmark;

[MemoryDiagnoser]
public class GW2BaseCacheBenchmark<T> where T : GW2APIBaseItem
{
    private GW2BaseCache<T> _gw2BaseCache = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gw2BaseCache = TestConstructorMemory();
    }

    [Benchmark]
    public GW2BaseCache<T> TestConstructorMemory()
    {
        string cacheName = typeof(T) switch
        {
            var t when t == typeof(GW2APISkill) => "./Content/SkillList.ndjson",
            var t when t == typeof(GW2APIMap) => "./Content/MapList.ndjson",
            _ => throw new NotSupportedException(
                $"No cache name configured for {typeof(T).Name}")
        };

        return new GW2BaseCache<T>(cacheName);
    }

    [Benchmark]
    [Arguments(1)]
    [Arguments(10)]
    [Arguments(100)]
    //[Arguments(1_000)]
    //[Arguments(10_000)]
    //[Arguments(100_000)]
    public async Task<T?> GetMultipleByIdAsync(int numberOfRuns)
    {
        T? result = null;

        for (int i = 0; i < numberOfRuns; i++)
        {
            result = await _gw2BaseCache.GetByIdAsync(i);
        }

        return result;
    }
}

public class GW2SkillCacheBenchmark : GW2BaseCacheBenchmark<GW2APISkill> { }
public class GW2MapCacheBenchmark : GW2BaseCacheBenchmark<GW2APIMap> { }
