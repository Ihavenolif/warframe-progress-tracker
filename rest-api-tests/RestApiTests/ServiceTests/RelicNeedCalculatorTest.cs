using rest_api.Services;

namespace rest_api_testing.ServiceTests;

public class RelicNeedCalculatorTest
{
    [Fact]
    public void MissingBlueprintAndDuplicateComponentsContributeRawQuantity()
    {
        var recipes = Recipes();
        var rewards = new HashSet<string> { "/RootBp", "/BladeBp" };

        var needs = RelicNeedCalculator.Calculate(rewards, recipes,
            new Dictionary<string, int>(), new HashSet<string>());

        Assert.Equal(1, needs["/RootBp"].Count);
        Assert.Equal(2, needs["/BladeBp"].Count);
        Assert.Equal(["Test Prime"], needs["/BladeBp"].RequiredFor);
    }

    [Fact]
    public void CraftedComponentAndBlueprintReduceNeed()
    {
        var inventory = new Dictionary<string, int>
        {
            ["/Blade"] = 1,
            ["/BladeBp"] = 1
        };

        var needs = RelicNeedCalculator.Calculate(
            new HashSet<string> { "/RootBp", "/BladeBp" }, Recipes(), inventory, new HashSet<string>());

        Assert.Equal(1, needs["/RootBp"].Count);
        Assert.DoesNotContain("/BladeBp", needs.Keys);
    }

    [Fact]
    public void MasteryRowIncludingZeroXpMeansGearOwned()
    {
        var needs = RelicNeedCalculator.Calculate(
            new HashSet<string> { "/RootBp", "/BladeBp" }, Recipes(),
            new Dictionary<string, int>(), new HashSet<string> { "/Root" });

        Assert.Empty(needs);
    }

    [Fact]
    public void NonPrimeGearAndResourcesAreIgnored()
    {
        var recipes = Recipes().Append(new RecommendationRecipe(
            "/NormalBp", "/Normal", "Normal Weapon", false, true,
            [new RecommendationIngredient("/Resource", 10)])).ToList();

        var needs = RelicNeedCalculator.Calculate(
            new HashSet<string> { "/RootBp", "/BladeBp", "/Resource", "/NormalBp" },
            recipes, new Dictionary<string, int>(), new HashSet<string>());

        Assert.DoesNotContain("/Resource", needs.Keys);
        Assert.DoesNotContain("/NormalBp", needs.Keys);
    }

    [Fact]
    public void SharedCraftedInventoryIsAllocatedOnceWithinTarget()
    {
        var recipes = new List<RecommendationRecipe>
        {
            new("/RootBp", "/Root", "Combined Prime", true, true,
                [new("/First", 1), new("/Second", 1)]),
            new("/FirstBp", "/First", "First Prime", true, false,
                [new("/Shared", 1)]),
            new("/SecondBp", "/Second", "Second Prime", true, false,
                [new("/Shared", 1)]),
            new("/SharedBp", "/Shared", "Shared Prime Part", true, false, [])
        };

        var needs = RelicNeedCalculator.Calculate(
            new HashSet<string> { "/SharedBp" }, recipes,
            new Dictionary<string, int> { ["/Shared"] = 1 }, new HashSet<string>());

        Assert.Equal(1, needs["/SharedBp"].Count);
    }

    private static List<RecommendationRecipe> Recipes()
    {
        return
        [
            new RecommendationRecipe("/RootBp", "/Root", "Test Prime", true, true,
                [new RecommendationIngredient("/Blade", 2)]),
            new RecommendationRecipe("/BladeBp", "/Blade", "Test Prime Blade", true, false,
                [new RecommendationIngredient("/Cell", 10)])
        ];
    }
}
