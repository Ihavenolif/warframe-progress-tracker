# Relic Data Model and PublicExport Import

## Goal

Store canonical Void Relics, refinement variants, and Prime rewards using exact identifiers from Warframe PublicExport.

This phase provides metadata only. Player ownership, relic browsing, and recommendations belong to later phases.

## Scope

- Support Lith, Meso, Neo, and Axi Void Relics.
- Exclude Requiem Relics and Arcanes.
- Import all relics, including currently vaulted relics.
- Store all four refinement variants:
  - `Bronze` suffix: Intact
  - `Silver` suffix: Exceptional
  - `Gold` suffix: Flawless
  - `Platinum` suffix: Radiant
- Store reward rarity. Drop probabilities can be derived from rarity and refinement later.
- Link Prime rewards to existing `Item` records.
- Keep import idempotent and capable of updating changed metadata.

## Source

Use `ExportRelicArcane` from Warframe PublicExport. Existing updater already downloads and parses PublicExport index in `data-update/data.py`.

Relevant fields:

```json
{
  "uniqueName": "/Lotus/Types/Game/Projections/T1VoidProjectionStyanaxPrimeBBronze",
  "name": "Lith A12 Relic",
  "relicRewards": [
    {
      "rewardName": "/Lotus/StoreItems/Types/Recipes/Weapons/AlternoxPrimeBlueprint",
      "rarity": "RARE",
      "itemCount": 1
    }
  ]
}
```

PublicExport reward paths use `/Lotus/StoreItems/...`; existing item and recipe records generally use `/Lotus/...`. Normalize reward identifiers by removing `StoreItems/`. Create a minimal `Item` row for rare rewards outside existing mastery metadata so every imported Prime reward remains FK-safe.

## Data Model

### `Relic`

Represents logical relic shared by all refinements.

- `Id`: generated primary key.
- `Name`: canonical display name without `Relic`, for example `Lith A12`.
- `Era`: enum or constrained string: Lith, Meso, Neo, Axi.
- Unique index on `Name`.

### `RelicVariant`

Represents inventory item for one refinement.

- `UniqueName`: PublicExport unique name, primary key.
- `RelicId`: foreign key to `Relic`.
- `Refinement`: Intact, Exceptional, Flawless, or Radiant.
- Index on `(RelicId, Refinement)`. PublicExport can expose multiple internal inventory IDs for one visible relic refinement.
- One-to-one or shared-primary-key relation to `Item` using `UniqueName`.

Every variant must also exist in `Item`:

- `name`: include refinement, for example `Lith A12 Intact`.
- `type`: `Relic`.
- `item_class`: `MiscItems`.
- `xp_required`: null.

Keeping variants in `Item` lets existing `Player_item` foreign key and import pipeline store relic ownership in phase 2.

### `RelicReward`

Represents one logical reward from one relic.

- `RelicId`: foreign key to `Relic`.
- `RewardUniqueName`: foreign key to `Item`.
- `Rarity`: Common, Uncommon, or Rare.
- `ItemCount`: normally 1, retained from source.
- Composite primary key on `(RelicId, RewardUniqueName)`.

Refinement does not belong here. All four variants contain same reward set; only probabilities differ.

## Import Work

### Parser

Extend `data-update/data.py`:

1. Fetch `index["RelicArcane"]`.
2. Read `ExportRelicArcane`.
3. Keep records whose names match `^(Lith|Meso|Neo|Axi) .+ Relic$`.
4. Determine refinement from unique-name suffix.
5. Strip trailing ` Relic` from canonical name.
6. Group variants by canonical name. Preserve duplicate internal aliases for a refinement.
7. Verify grouped variants expose identical reward sets and rarities.
8. Normalize reward unique names.
9. Ignore Forma and other non-Prime rewards.
10. Return canonical relics, variants, and rewards as separate collections.

Do not silently ignore malformed refinement suffixes or inconsistent reward sets. Prime rewards outside existing mastery metadata receive minimal item records and remain available for browsing.

### Database Sync

Extend `data-update/pg_send_data.py`:

1. Import normal gear, resources, and recipes first.
2. Insert or update relic variant `Item` rows.
3. Upsert canonical relics.
4. Upsert variants.
5. Replace or reconcile rewards for imported relics.
6. Commit relic metadata in one transaction.

Avoid `ON CONFLICT DO NOTHING` for relic metadata. Names, links, and reward sets must update when source changes.

### EF Core

- Add models and `DbSet` properties.
- Configure keys, indexes, relationships, lengths, and delete behavior in `WarframeTrackerDbContext`.
- Add migration creating relic tables and indexes.
- Preserve existing `Item` and `Player_item` schema.

## Validation

- Every canonical relic has at least one variant for each of four refinements.
- Every canonical relic has six rewards after filtering only when all six are in scope; otherwise retain full reward rows or explicitly document filtered count. Preferred implementation stores all mapped rewards and marks recommendation eligibility separately.
- Every Prime reward resolves to existing `Item`.
- Variant unique names remain stable across repeated updates.
- Re-running importer creates no duplicates.
- Changed source data updates existing rows.

## Tests

- Parser maps Bronze/Silver/Gold/Platinum correctly.
- Parser groups refinement records and internal aliases into one canonical relic.
- `/Lotus/StoreItems/...` normalization resolves existing item IDs.
- Requiem and Arcane records are excluded.
- Forma does not become a Prime recommendation reward.
- Inconsistent variant reward sets fail with relic name in error.
- Database sync is idempotent.
- EF migration applies on PostgreSQL.

## Acceptance Criteria

- Database contains canonical Lith/Meso/Neo/Axi relics, all PublicExport variants, and linked rewards.
- Relic variants exist in `Item` and can be referenced by `Player_item`.
- Admin metadata update refreshes relic data without manual SQL.
- No Prime reward remains unmapped without visible import failure.
- Backend and updater tests pass.

## Out of Scope

- Player relic ownership import.
- Relic CRUD API and browsing UI.
- Mission drop locations and vaulted/farmable status.
- Recommendation scoring.
