# Relic API load test

Run against a Release API and production-like PostgreSQL database. Endpoint requires a user with a linked player.

Use an existing token:

```powershell
$env:ACCESS_TOKEN = '<JWT>'
k6 run "rest-api-tests\k6\relics.js"
```

Or let setup log in once:

```powershell
$env:USERNAME = '<username>'
$env:PASSWORD = '<password>'
k6 run "rest-api-tests\k6\relics.js"
```

Override load and export a summary:

```powershell
$env:VUS = '20'
$env:DURATION = '2m'
$env:QUERY = 'owned=all&sort=name&page=1&pageSize=100'
k6 run --summary-export "rest-api-tests\k6\results\summary.json" "rest-api-tests\k6\relics.js"
```

Optional `BASE_URL` defaults to `http://localhost:5224`. Initial thresholds cover failed requests and checks only. Establish stable same-machine baselines before adding latency regression thresholds.
