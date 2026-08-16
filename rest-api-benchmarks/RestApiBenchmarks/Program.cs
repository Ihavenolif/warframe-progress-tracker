using BenchmarkDotNet.Running;
using RestApiBenchmarks;

BenchmarkSwitcher.FromAssembly(typeof(RelicNeedCalculatorBenchmarks).Assembly).Run(args);
