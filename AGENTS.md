# Repository Agent Guide

## Scope

- Actively maintained areas are `rest-api`, `rest-api-tests`, `vue-mpa`, `data-update`, and `alecaframe-parser`.
- Treat `flaskapp` as legacy and frozen. Change it only when a task explicitly names it.
- Treat `plans/*.md` as authoritative only when the user names a plan or the task clearly implements it.
- Keep changes local to the requested area. Do not combine feature work with broad cleanup or style migration.

## Repository Layout

- `rest-api`: ASP.NET Core API targeting .NET 9.
- `rest-api-tests/RestApiTests`: xUnit API tests.
- `vue-mpa`: Vue 3 SPA built with Vue CLI.
- `data-update`: Python data updater and its `unittest` suite.
- `alecaframe-parser`: Python desktop parser and PyInstaller build.
- `k8s`, `nginx`, `imgproxy`, and root Docker Compose files: deployment configuration.
- `warframe-tracker.sln`: primary .NET solution.

## General Conventions

- Follow existing architecture and file placement. Prefer smallest correct change.
- Treat identifiers matching `ORB-X` (for example, `ORB-123`) as Linear issues. Fetch them through Linear MCP tools before acting on them.
- Preserve public contracts, persisted data, database schema, and deployment behavior unless task requires change.
- Use ASCII for new text unless surrounding file requires Unicode.
- Keep secrets and machine-local configuration out of Git. Never read, print, or commit `.env`, `database.ini`, certificates, or keys.
- Add comments only when intent is not clear from code.
- Do not introduce formatter, linter, framework, package manager, or dependency changes without task need.

## Linear Workflow

- When the user supplies a Linear issue identifier, move the issue to `In Progress` before starting work.
- Format every commit for the issue as `ORB-X type(scope): message`, using the full issue identifier. Example: `ORB-123 feat(api): add relic lookup endpoint`.
- After the work is finished and committed, ask the user before moving the issue to `Done`.

## C# Conventions

- Keep API code in technical layers: `Controllers`, `Services`, `DTOs`, `Models`, and `Data`.
- Use file-scoped namespaces, four-space indentation, nullable-aware code, and implicit usings.
- Use PascalCase for types, methods, properties, and public members; use camelCase for locals and parameters; use `_camelCase` for private fields.
- Suffix asynchronous methods with `Async`.
- Name new DTO types with `Dto`, not `DTO`.
- Use PascalCase C# names for database-backed members. Map snake_case database names explicitly through EF configuration; do not spread database naming into new C# APIs.
- Keep controllers thin. Put application and persistence work in injected services.
- Keep service interface and implementation together when that matches nearby services.
- Put EF model configuration in `Data/WarframeTrackerDbContext.cs` and track intentional migrations plus model snapshots.
- Preserve nearby `using` grouping when editing; remove unused imports.

## Vue And JavaScript Conventions

- Use Vue 3 Options API unless task explicitly calls for a broader migration.
- Use PascalCase component filenames. Put route pages under `vue-mpa/src/routes` and reusable UI under `vue-mpa/src/components`.
- Group feature components in PascalCase directories.
- Use single quotes and semicolons in new or edited JavaScript.
- Prefer the `@/` alias for imports from `src`; use relative imports for files within the same small feature folder.
- Keep routes centralized and lazy-loaded in `src/router/index.js`.
- Keep shared state in the existing Vuex store pattern.
- Put global CSS in `src/assets/styles`. Preserve existing cascade-layer order and reuse tokens from `tokens.css`.
- Use kebab-case CSS classes; use BEM-style suffixes for component parts when useful.

## Python Conventions

- Follow PEP 8: four-space indentation, snake_case functions and variables, PascalCase classes, and UPPER_CASE constants.
- Group imports as standard library, third-party, then local imports. Avoid wildcard imports in new code.
- Add type hints to new public functions and changed public signatures where practical. Do not force unrelated annotation churn.
- Keep `data-update` tests on standard-library `unittest`.
- Keep parser packaging changes aligned with `alecaframe-parser/app.spec` and `build.ps1`.

## Tests And Validation

- Add or update focused tests when behavior changes. Match nearby test structure and use descriptive behavior names.
- Before running a build or starting a development server, check for an existing local or Docker-hosted server. Reuse it when suitable; do not start a duplicate.
- On Windows, inspect process command lines for `npm run serve`, `vue-cli-service serve`, `dotnet run`, `dotnet watch`, and `python -u server.py`. Ignore unrelated Node and .NET tooling such as language servers, Playwright MCP, and IDE build hosts.
- Run `docker ps` and look for this repository's Compose services: `warframe-progress-tracker-frontend-1`, `warframe-progress-tracker-backend-1`, and `warframe-progress-tracker-data-update-1` (or equivalent names with Compose project/service labels).
- Check listeners on the normal development ports: frontend `8080`, API `5224` (and local HTTPS `7245`), and updater `5000`. Vue CLI can automatically select `8081` or a higher port when `8080` is occupied, so process command lines must also be checked.
- API or C# changes: run `dotnet test warframe-tracker.sln` from repository root.
- Vue changes: run `npm run lint` and `npm run build` from `vue-mpa`.
- Updater changes: run `python -m unittest discover -s tests` from `data-update`.
- Parser changes: run `python -m py_compile app.py` from `alecaframe-parser`; run its build only when packaging changes.
- Deployment changes: render or build the affected Docker, Compose, or Kustomize target when local tooling permits.
- Report checks not run and reason. Do not claim validation from unrelated checks.

## Generated And Tracked Artifacts

- Do not edit, regenerate, delete, or normalize tracked downloaded data, `.lzma` files, binaries, archives, root JSON/SQL files, or cache artifacts unless task explicitly requires them.
- Do not commit ignored outputs such as `node_modules`, `dist`, `build`, `bin`, `obj`, `__pycache__`, parser output, or local environment files.
- Do not hand-edit generated EF migration designer files or model snapshots except as part of an intentional migration workflow.

## Git Conventions

- Match recent commit history: prefer concise Conventional Commit-style subjects such as `feat(frontend): ...`, `fix: ...`, or `refactor(api): ...`, but do not enforce this on existing history.
- Do not use `dev` as a commit prefix.
- Never revert or overwrite unrelated working-tree changes.
- Stage only files belonging to the requested change.
