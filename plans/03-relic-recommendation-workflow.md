# Relic Recommendation Workflow

## Goal

Let user select 1-4 clan members and rank owned relics by how many missing Prime craft requirements their rewards cover.

Depends on phase 1 relic metadata and phase 2 player relic ownership.

## Product Decisions

- Squad source: selected clan members.
- Squad size: 1-4 distinct players.
- Need definition: missing craft quantity after owned blueprints, components, and crafted items.
- Candidate ownership: relic qualifies when any selected player owns at least one variant.
- Ranking: raw need-point sum; reward probability does not affect score.
- Scope: Prime gear and relic rewards only.
- A player with gear represented in `Player_items_mastery`, including XP 0, already owns gear and needs no parts for it.
- Result shows which player owns recommended relic and which players need each reward.

## Need Calculation

For every selected player and every unowned Prime gear item:

1. Find gear recipe.
2. Add one need for missing main blueprint.
3. Traverse recipe ingredients.
4. Subtract owned crafted component quantity.
5. For craftable Prime components, subtract owned component blueprint quantity and continue into nested Prime prerequisites where needed.
6. Ignore non-relic resources such as Orokin Cells.
7. Keep only missing items present in imported relic reward catalog.

Base formula:

```text
need(player, reward) = max(required quantity - usable owned quantity, 0)

relic score = sum(
    need(player, reward)
    for every selected player
    for every reward in relic
)
```

Raw quantity matters. Player missing two identical blades contributes two points.

Prevent double-counting inventory used across nested recipes. Recipe traversal should allocate owned quantities within each target build consistently. If exact global crafting optimization becomes too complex, document target-by-target behavior and cover known shared-component Prime recipes with tests.

## Candidate Relics

1. Group inventory variants under canonical relic.
2. Sum each selected player's counts by refinement.
3. Keep relic when any selected player owns at least one copy.
4. Keep relic when score is greater than zero.
5. Do not require all squad members to own same relic.

Ownership does not add score. It controls eligibility and tells squad who can equip relic.

## Ordering

Sort recommendations by:

1. Score descending.
2. Number of benefiting players descending.
3. Number of distinct needed rewards descending.
4. Relic name ascending.

Deterministic tie-breaking prevents results moving between identical requests.

## API

Add authenticated endpoint:

```http
POST /api/clans/{clanName}/relic-recommendations
```

Request:

```json
{
  "playerIds": [12, 18, 24]
}
```

Validation:

- Requester belongs to clan.
- 1-4 player IDs supplied.
- IDs are distinct.
- Every selected player belongs to same clan.
- Every selected player has imported profile data.

Response:

```json
{
  "players": [
    { "id": 12, "name": "PlayerOne" }
  ],
  "recommendations": [
    {
      "relicId": 123,
      "name": "Lith A12",
      "era": "Lith",
      "score": 5,
      "benefitingPlayerCount": 3,
      "owners": [
        {
          "playerId": 12,
          "playerName": "PlayerOne",
          "totalCount": 2,
          "refinements": {
            "intact": 1,
            "exceptional": 0,
            "flawless": 0,
            "radiant": 1
          }
        }
      ],
      "usefulRewards": [
        {
          "itemName": "Daikyu Prime Lower Limb",
          "itemUniqueName": "/Lotus/Types/Recipes/Weapons/WeaponParts/PrimeDaikyuLowerLimb",
          "rarity": "Uncommon",
          "needPoints": 3,
          "players": [
            {
              "playerId": 12,
              "playerName": "PlayerOne",
              "missingCount": 1,
              "requiredFor": ["Daikyu Prime"]
            }
          ]
        }
      ]
    }
  ]
}
```

Return top results with configurable bounded limit, default 10. Full ranking can be paginated later.

## Backend Structure

- Add `IRelicRecommendationService` and `RelicRecommendationService`.
- Keep recommendation DTOs separate from relic browser DTOs.
- Reuse clan authorization patterns from `ClanController`.
- Use IDs internally; names remain display data only.
- Query needed player inventories and mastery rows in batches.
- Avoid calling current PostgreSQL materialized-view query once per player.
- Build recipe/reward lookup once per request, then calculate needs in memory or with bounded database projections.

Potential reusable structures:

- Reward unique-name set.
- Recipe graph keyed by result item.
- Player inventory dictionary keyed by item unique name.
- Player-owned mastery item set.
- Relic ownership grouped by canonical relic and refinement.

## Frontend Workflow

Extend `vue-mpa/src/routes/Clans/Progress.vue` or add child recommendation component beside existing progress table.

### Interaction

1. User opens clan progress.
2. User selects 1-4 members.
3. `Recommend relics` button becomes enabled.
4. Button sends selected player IDs.
5. Ranked results appear without replacing progress table.
6. Changing selection marks old results stale or clears them.

### Result Card

Collapsed view:

- Relic name and era.
- Need-point score.
- Number of players helped.
- Owners and total available copies.

Expanded view:

- Counts by refinement and owner.
- Useful rewards grouped by rarity.
- Missing quantity per player.
- Prime gear requiring each reward.
- Explanation: score equals total missing quantities covered, not drop chance.

States:

- No members selected.
- More than four selection prevented in UI.
- Loading.
- API error.
- No imported inventory.
- No owned useful relics.
- Recommendations available.

Use existing progress visual language and responsive table/card patterns. On mobile, member selector and reward details stack vertically.

## Tests

### Need Engine

- Missing main blueprint contributes one point.
- Owned blueprint contributes zero.
- Required duplicate component uses remaining quantity.
- Crafted component and component blueprint reduce needs correctly.
- XP-tracked Prime gear contributes no needs.
- Non-Prime gear and non-relic resources are ignored.
- Nested Prime weapon prerequisites resolve relic rewards.
- Shared inventory is not incorrectly counted twice inside one craft calculation.

### Candidate and Ranking

- Relic owned by one selected player qualifies.
- Relic owned only by unselected clan member does not qualify.
- Refinement variants merge under canonical relic.
- Ownership count does not alter raw score.
- Zero-score relic is excluded.
- Tie-breakers are deterministic.

### Authorization and Validation

- Non-member cannot request clan recommendations.
- Selected outsider is rejected.
- Empty, duplicate, and more-than-four player lists are rejected.
- Missing profile data returns actionable response.

### Frontend

- Selection is capped at four.
- Request contains selected IDs.
- Results render owner/refinement and need details.
- Empty and error states render correctly.
- Vue production build succeeds.

## Acceptance Criteria

- Clan member can select 1-4 clan players and request recommendations.
- Only relics owned by selected players are returned.
- Score equals raw missing Prime craft quantities covered by relic rewards.
- Response explains every score through per-player reward needs.
- Owners and exact refinement counts are visible.
- Authorization prevents cross-clan inventory access.
- Backend tests and Vue production build pass.

## Future Work

- Chance-weighted or exact squad utility ranking.
- Refinement advice and Void Trace cost.
- Current mission drop locations for acquiring unowned relics.
- Vaulted, Baro, and Prime Resurgence status.
- Mixed-relic squad loadout optimization.
- Market value and Ducat fallback ranking.
