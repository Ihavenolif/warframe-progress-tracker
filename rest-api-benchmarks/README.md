# REST API benchmarks

Performance harness has two BenchmarkDotNet executables.

## Method benchmarks

Run CPU and allocation benchmarks without external services:

```powershell
dotnet run -c Release --project "rest-api-benchmarks\RestApiBenchmarks" -- --filter "*RelicNeedCalculator*"
```

## PostgreSQL benchmarks

Docker must be running. Testcontainers starts an isolated PostgreSQL 16 container. Global setup applies migrations, derives a deterministic catalog from the three `data-update/out_*.json` files, creates three users and players in one clan, and imports each profile sequentially through `MasteryService.UpdatePlayerMasteryAsync`.

Container startup, migrations, catalog setup, user setup, and profile imports are outside measured iterations.

```powershell
dotnet run -c Release --project "rest-api-benchmarks\RestApiPostgreSqlBenchmarks" -- --filter "*ClanMastery*"
```

BenchmarkDotNet writes reports under `BenchmarkDotNet.Artifacts`. Compare reports from same machine under similar load. Do not treat results from SQLite or Debug builds as production database measurements.
