# Player Relic Import, CRUD, and Browser

## Goal

Import player-owned relic counts, expose relic metadata and ownership through API, and add frontend relic browser.

Depends on phase 1 relic metadata and variant `Item` rows.

## Scope

- Import owned relic quantities from AlecaFrame profile JSON.
- Preserve counts separately for every refinement.
- Browse and filter all known relics.
- Show current player's owned quantities.
- Provide admin CRUD for correcting relic metadata.
- Do not calculate recommendations yet.

## Player Import

Current `MasteryService.UpdatePlayerMasteryAsync` imports `MiscItems` only when `ItemType` exists in `Item`. Phase 1 makes relic variants valid items, so existing filtering can retain them.

Required changes:

1. Confirm AlecaFrame exports relic variants in `MiscItems` using PublicExport unique names.
2. Add import fixture containing Intact, Exceptional, Flawless, and Radiant relics.
3. Verify each entry becomes `Player_item` with exact count.
4. Ensure repeated profile import updates counts rather than adding duplicates.
5. Remove stale relic ownership absent from latest full profile snapshot, or explicitly define imports as incremental. Preferred behavior: synchronize imported inventory because recommendation must not use consumed relics.
6. Keep non-relic inventory behavior unchanged unless snapshot reconciliation is deliberately generalized.

Existing users must upload profile JSON again after relic metadata deployment. Raw source imports are not stored, so old relic ownership cannot be recovered automatically.

## API Model

Create dedicated DTOs instead of exposing EF entities.

### Relic Summary

- ID.
- Name.
- Era.
- Rewards grouped once per canonical relic.
- Variants with refinement and unique name.
- Current-player quantity for each variant.
- Total owned quantity.

### Relic Detail

- Summary fields.
- Full reward list with item name, unique name, rarity, and count.
- Variant metadata and current-player ownership.

## Read API

Add authenticated endpoints under `api/relics`:

```http
GET /api/relics
GET /api/relics/{id}
```

List query parameters:

- `search`: relic or reward name.
- `era`: Lith, Meso, Neo, Axi.
- `refinement`: optional ownership filter.
- `owned`: all, owned, unowned.
- `page` and `pageSize`.
- `sort`: name, era, owned count.

Server-side pagination avoids returning hundreds of relics plus all ownership rows on every request.

Authorization:

- Any linked authenticated player may browse metadata and own counts.
- Requests without linked player may browse metadata only if product behavior permits; otherwise return existing `Player not found` response.
- Never expose another player's inventory through these endpoints.

## Admin CRUD

Add admin-authorized endpoints:

```http
POST   /api/relics
PUT    /api/relics/{id}
DELETE /api/relics/{id}
POST   /api/relics/{id}/variants
PUT    /api/relics/{id}/variants/{uniqueName}
DELETE /api/relics/{id}/variants/{uniqueName}
POST   /api/relics/{id}/rewards
PUT    /api/relics/{id}/rewards/{rewardUniqueName}
DELETE /api/relics/{id}/rewards/{rewardUniqueName}
```

CRUD exists for operational correction, but PublicExport remains source of truth. Next metadata sync may overwrite manual changes. UI must communicate this.

Deletion rules:

- Prevent deleting variant referenced by player inventory unless ownership rows are explicitly handled.
- Prefer deactivation over destructive deletion when imported source removes data.
- Validate four unique refinements per complete canonical relic.
- Validate rewards reference existing items.

## Backend Structure

- Add `IRelicService` and `RelicService`.
- Register service in `Program.cs`.
- Add `RelicController`.
- Keep ownership joins and filtering in service/query layer.
- Use `AsNoTracking` for browser queries.
- Project directly to DTOs to avoid loading all player inventories.

## Frontend Browser

Add route:

```text
/relics
```

Add navigation entry following existing layout conventions.

### Browser Layout

- Search input.
- Era chips/tabs.
- Owned/all filter.
- Relic cards or responsive table.
- Owned total visible in collapsed row.
- Expandable details showing:
  - Intact, Exceptional, Flawless, Radiant counts.
  - Six rewards.
  - Common, Uncommon, Rare labels.
- Pagination controls.
- Loading, error, empty, and no-player states.

Use existing colors and components from progress pages. Preserve mobile usability; expanded rewards should stack rather than force wide horizontal scrolling.

### Admin UI

Extend existing admin panel or add admin-only controls in relic detail:

- Create/edit canonical relic.
- Manage variants.
- Manage rewards.
- Confirm destructive actions.
- Warn that PublicExport sync may overwrite edits.

## Tests

### Import

- All four refinements import with independent counts.
- Re-import updates counts.
- Consumed relic removed from full snapshot becomes zero/absent.
- Unknown relic does not break entire player import; log or reject according to existing import policy.
- Normal recipe and resource imports remain unchanged.

### API

- Current player sees only own counts.
- Search matches relic and reward names.
- Era, owned, sorting, and pagination filters compose correctly.
- Non-admin cannot mutate relic metadata.
- Admin CRUD validates duplicate names, variants, and rewards.
- Referenced variant cannot be destructively deleted.

### Frontend

- Production build succeeds.
- Browser handles loading/error/empty states.
- Filters update query and results.
- Detail view shows quality-specific ownership.
- Mobile layout remains usable.

## Acceptance Criteria

- Fresh player profile import stores accurate relic counts by refinement.
- Consumed relics do not remain falsely owned after next import.
- Player can browse all relics and see own inventory.
- Admin can inspect and correct relic metadata through authorized CRUD.
- Ownership APIs do not leak other players' inventory.
- Backend tests and Vue production build pass.

## Out of Scope

- Clan member inventory visibility.
- Recommendation scoring or member selection.
- Relic mission drop locations.
- Market prices and Ducat optimization.
