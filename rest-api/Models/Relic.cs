namespace rest_api.Models;

public enum RelicEra
{
    Lith,
    Meso,
    Neo,
    Axi
}

public enum RelicRefinement
{
    Intact,
    Exceptional,
    Flawless,
    Radiant
}

public enum RelicRewardRarity
{
    Common,
    Uncommon,
    Rare
}

public class Relic
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public RelicEra Era { get; set; }

    public virtual ICollection<RelicVariant> Variants { get; set; } = new List<RelicVariant>();

    public virtual ICollection<RelicReward> Rewards { get; set; } = new List<RelicReward>();
}
