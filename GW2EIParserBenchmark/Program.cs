using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using GW2EIGW2API.GW2API;
using GW2EIParserBenchmark;

//BenchmarkRunner.Run<EvctParserBenchmark>(new DebugInProcessConfig());
BenchmarkRunner.Run<GW2SkillCacheBenchmark>();
//BenchmarkRunner.Run<GW2MapCacheBenchmark>();
