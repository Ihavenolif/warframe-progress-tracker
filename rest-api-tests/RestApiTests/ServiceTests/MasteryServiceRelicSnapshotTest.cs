using rest_api.Models;
using rest_api.Services;

namespace rest_api_testing.ServiceTests;

public class MasteryServiceRelicSnapshotTest
{
    private const string Intact = "/Lotus/Types/Game/Projections/TestPrimeBronze";
    private const string Exceptional = "/Lotus/Types/Game/Projections/TestPrimeSilver";
    private const string Flawless = "/Lotus/Types/Game/Projections/TestPrimeGold";
    private const string Radiant = "/Lotus/Types/Game/Projections/TestPrimePlatinum";
    private const string Unknown = "/Lotus/Types/Game/Projections/UnknownPrimeBronze";
    private const string NonRelic = "/Lotus/NonRelic";

    [Fact]
    public void ImportsKnownVariantsIndependentlyWithExactPositiveCounts()
    {
        HashSet<string> knownRelics = [Intact, Exceptional, Flawless, Radiant];

        var result = MasteryService.ReconcileRelicSnapshot(
            7,
            [(Intact, 2), (Exceptional, 5), (Flawless, 7), (Radiant, 11), (Unknown, 9), (NonRelic, 3)],
            knownRelics,
            [.. knownRelics, NonRelic],
            []);

        Dictionary<string, Player_item> entries = result.RelicEntries.ToDictionary(item => item.unique_name);
        Assert.Equal(4, entries.Count);
        Assert.Equal(2, entries[Intact].item_count);
        Assert.Equal(5, entries[Exceptional].item_count);
        Assert.Equal(7, entries[Flawless].item_count);
        Assert.Equal(11, entries[Radiant].item_count);
        Assert.All(entries.Values, item => Assert.Equal(7, item.player_id));
        Assert.Equal([Unknown], result.UnknownProjectionUniqueNames);
    }

    [Fact]
    public void RepeatedSnapshotUpdatesCountsAndDeletesAbsentOrZeroVariantsOnly()
    {
        HashSet<string> knownRelics = [Intact, Exceptional, Flawless];
        var existing = new[]
        {
            PlayerItem(Intact, 1),
            PlayerItem(Exceptional, 4),
            PlayerItem(Flawless, 8)
        };

        var result = MasteryService.ReconcileRelicSnapshot(
            7,
            [(Intact, 11), (Exceptional, 0), (NonRelic, 0)],
            knownRelics,
            [.. knownRelics, NonRelic],
            existing);

        Player_item updated = Assert.Single(result.RelicEntries);
        Assert.Equal(Intact, updated.unique_name);
        Assert.Equal(11, updated.item_count);
        Assert.True(new HashSet<string> { Exceptional, Flawless }
            .SetEquals(result.StaleEntries.Select(item => item.unique_name)));
        Player_item nonRelic = Assert.Single(result.NonRelicEntries);
        Assert.Equal(NonRelic, nonRelic.unique_name);
        Assert.Equal(0, nonRelic.item_count);
    }

    [Fact]
    public void FullInventorySnapshotDeletesAbsentRecipesAndComponents()
    {
        Player_item retainedRecipe = PlayerItem("/Lotus/Recipe/Retained", 2);
        Player_item retainedComponent = PlayerItem("/Lotus/Component/Retained", 3);
        Player_item staleRecipe = PlayerItem("/Lotus/Recipe/Stale", 1);
        Player_item staleComponent = PlayerItem("/Lotus/Component/Stale", 4);

        List<Player_item> stale = MasteryService.FindStaleInventoryEntries(
            [retainedRecipe, retainedComponent],
            [retainedRecipe, retainedComponent, staleRecipe, staleComponent]);

        Assert.True(new HashSet<string> { staleRecipe.unique_name, staleComponent.unique_name }
            .SetEquals(stale.Select(item => item.unique_name)));
    }

    private static Player_item PlayerItem(string uniqueName, int count) => new()
    {
        unique_name = uniqueName,
        player_id = 7,
        item_count = count
    };
}
