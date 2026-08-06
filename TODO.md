# Project Risks

## Critical

- [ ] Stop sending login passwords in URL query parameters.
  - Evidence: `vue-mpa/src/components/Forms/LoginForm.vue` builds and logs a login URL containing the password.
  - Risk: browser history, proxy logs, server logs, and diagnostics can expose credentials.
  - Done when: login accepts credentials only in a request body, URLs contain no secrets, and login requests are not logged with credentials.

- [ ] Add object-level authorization to mastery lookups.
  - Evidence: `rest-api/Controllers/MasteryController.cs` exposes mastery by username and contains an authorization TODO.
  - Risk: authenticated users may read another player's private progression data.
  - Done when: access policy is explicit, enforced server-side, and covered by allowed/denied tests.

- [ ] Restrict and authenticate the data updater.
  - Evidence: `data-update/server.py` exposes an update endpoint while `docker-compose.yml` publishes port `5000`.
  - Risk: network clients can trigger expensive downloads and database writes.
  - Done when: updater is reachable only by trusted services or requires strong authentication, and concurrent updates are rejected or serialized.

## High

- [x] Remove production certificate loading from the backend.
  - Resolution: TLS terminates at the environment-managed edge; the backend no longer loads an unused local PKCS#12 file.
  - Verified by: backend container image build passes without certificate files in its build context.

- [ ] Move browser authentication tokens out of `localStorage`.
  - Evidence: `vue-mpa/src/store.js` persists JWT data in `localStorage`.
  - Risk: successful XSS can steal reusable tokens.
  - Done when: authentication uses secure, `HttpOnly`, `SameSite` cookies or an equally justified design, with CSRF handling documented and tested.

- [ ] Hash persisted refresh tokens.
  - Evidence: `rest-api/Services/TokenService.cs` stores refresh-token values directly.
  - Risk: database disclosure grants active sessions until token expiry or revocation.
  - Done when: only token hashes are stored and rotation, lookup, expiry, and revocation tests pass.

- [ ] Fix or remove the missing PostgreSQL initialization mount.
  - Evidence: `docker-compose.yml` references `./create.sql`, but that file is absent.
  - Risk: fresh Compose deployments can fail or start without expected initialization.
  - Done when: required script exists and is tested, or obsolete mount is removed.

- [ ] Make metadata updates reconcile changed and removed records.
  - Evidence: `data-update/pg_send_data.py` uses insert-only `ON CONFLICT DO NOTHING` behavior.
  - Risk: renamed, changed, or deleted upstream data remains stale indefinitely.
  - Done when: updates use deterministic upserts and stale-record policy is defined and tested.

- [ ] Verify blueprint trigger key lookup.
  - Evidence: migration `20260108183615_AddTriggersAndIndexes.cs` compares `item.unique_name` with `NEW.name`.
  - Risk: blueprint relationships may reference wrong items or fail to populate.
  - Done when: intended key is confirmed, corrected migration is added if needed, and PostgreSQL integration coverage proves behavior.

## Medium

- [ ] Control automatic database migrations during API startup.
  - Evidence: `rest-api/Program.cs` applies EF migrations on every startup.
  - Risk: concurrent replicas, unavailable databases, or long migrations can prevent service startup.
  - Done when: deployment owns migration ordering, readiness behavior is explicit, and migration failure is observable.

- [ ] Add validation for registration and mastery imports.
  - Evidence: TODOs remain in `rest-api/Controllers/AuthController.cs` and `rest-api/Services/MasteryService.cs`.
  - Risk: malformed or oversized input can produce bad data, excessive work, or unclear failures.
  - Done when: input shape, size, ranges, and failure responses are defined and tested.

- [ ] Remove synchronous blocking from item index endpoint.
  - Evidence: `rest-api/Controllers/ItemController.cs` accesses asynchronous work through `.Result`.
  - Risk: thread-pool starvation and avoidable request latency under load.
  - Done when: endpoint awaits asynchronous calls end-to-end.

- [ ] Resolve unimplemented item service methods.
  - Evidence: two methods in `rest-api/Services/ItemService.cs` throw `NotImplementedException`.
  - Risk: callers can encounter runtime failures as functionality expands.
  - Done when: methods are implemented and tested, or removed from the service contract.

- [ ] Harden Public Export parsing and transport.
  - Evidence: updater parsing relies on fragile string splitting and includes plain HTTP manifest URLs.
  - Risk: upstream format changes break updates; insecure transport permits tampering where HTTP remains accepted.
  - Done when: structured parsing is used, HTTPS is enforced, and representative fixtures cover format changes and failures.

## Testing and Delivery

- [ ] Run tests and lint in CI before publishing images.
  - Evidence: `.github/workflows/ci-build.yml` builds and pushes images without test or lint jobs.
  - Risk: regressions can ship in valid container images.
  - Done when: backend tests and frontend lint gate image publication.

- [ ] Expand backend coverage beyond clan services.
  - Evidence: substantive tests currently cover only `ClanService`; placeholder `UnitTest1.cs` always passes.
  - Risk: authentication, token rotation, mastery imports, authorization, and SQL-specific behavior can regress unnoticed.
  - Done when: highest-risk auth and mastery paths have unit/integration coverage and placeholder test is removed.

- [ ] Add PostgreSQL integration tests.
  - Evidence: current tests use SQLite in-memory while production uses PostgreSQL, triggers, bulk operations, and a materialized view.
  - Risk: provider-specific SQL and schema behavior remain unverified.
  - Done when: CI exercises migrations, triggers, bulk imports, and materialized-view reads against PostgreSQL.

- [ ] Add frontend and updater tests.
  - Evidence: no frontend test script or Python updater test suite exists.
  - Risk: token refresh, import UI, parsers, normalization, and database update behavior can regress unnoticed.
  - Done when: critical browser auth/import flows and updater transformations have automated tests.
