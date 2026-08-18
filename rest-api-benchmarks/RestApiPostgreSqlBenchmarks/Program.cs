using BenchmarkDotNet.Running;
using RestApiPostgreSqlBenchmarks;

BenchmarkSwitcher.FromAssembly(typeof(ClanMasteryPostgreSqlBenchmarks).Assembly).Run(args);
