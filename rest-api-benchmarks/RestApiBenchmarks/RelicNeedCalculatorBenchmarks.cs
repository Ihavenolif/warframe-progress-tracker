using BenchmarkDotNet.Attributes;
using rest_api.Services;

namespace RestApiBenchmarks;

[MemoryDiagnoser]
public class RelicNeedCalculatorBenchmarks
{
    private HashSet<string> _rewardUniqueNames = null!;
    private List<RecommendationRecipe> _recipes = null!;
    private Dictionary<string, int> _inventory = null!;
    private HashSet<string> _masteredItems = null!;

    [Params(10, 100, 1000)]
    public int TargetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _rewardUniqueNames = new HashSet<string>(StringComparer.Ordinal);
        _recipes = new List<RecommendationRecipe>(TargetCount * 4);
        _inventory = new Dictionary<string, int>(StringComparer.Ordinal);
        _masteredItems = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < TargetCount; index++)
        {
            var target = $"/Benchmark/PrimeGear/{index}";
            var targetBlueprint = $"{target}/Blueprint";
            var ingredients = new List<RecommendationIngredient>(3);

            _rewardUniqueNames.Add(targetBlueprint);
            for (var componentIndex = 0; componentIndex < 3; componentIndex++)
            {
                var component = $"{target}/Component/{componentIndex}";
                var componentBlueprint = $"{component}/Blueprint";

                ingredients.Add(new RecommendationIngredient(component, 2));
                _rewardUniqueNames.Add(componentBlueprint);
                _recipes.Add(new RecommendationRecipe(
                    componentBlueprint,
                    component,
                    $"Benchmark Prime Component {index}-{componentIndex}",
                    IsPrime: true,
                    IsGear: false,
                    []));

                if ((index + componentIndex) % 3 == 0)
                {
                    _inventory[component] = 1;
                }
            }

            _recipes.Add(new RecommendationRecipe(
                targetBlueprint,
                target,
                $"Benchmark Prime Gear {index}",
                IsPrime: true,
                IsGear: true,
                ingredients));
            if (index % 10 == 0)
            {
                _masteredItems.Add(target);
            }
        }
    }

    [Benchmark]
    public object CalculateNeeds()
    {
        return RelicNeedCalculator.Calculate(
            _rewardUniqueNames,
            _recipes,
            _inventory,
            _masteredItems);
    }
}
