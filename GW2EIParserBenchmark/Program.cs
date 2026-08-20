using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using GW2EIParserBenchmark;

BenchmarkRunner.Run<EvctParserBenchmark>(new DebugInProcessConfig());
