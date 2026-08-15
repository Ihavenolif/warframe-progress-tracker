namespace rest_api.Models;

public class RelicReward
{
    public int RelicId { get; set; }

    public string RewardUniqueName { get; set; } = null!;

    public RelicRewardRarity Rarity { get; set; }

    public int ItemCount { get; set; } = 1;

    public virtual Relic Relic { get; set; } = null!;

    public virtual Item Reward { get; set; } = null!;
}
