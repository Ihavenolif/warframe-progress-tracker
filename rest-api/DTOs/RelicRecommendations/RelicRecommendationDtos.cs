namespace rest_api.DTOs.RelicRecommendations;

public class RelicRecommendationRequestDto
{
    public List<int> PlayerIds { get; set; } = new();
}

public class RelicRecommendationResponseDto
{
    public List<RelicRecommendationPlayerDto> Players { get; set; } = new();
    public List<RelicRecommendationDto> Recommendations { get; set; } = new();
}

public class RelicRecommendationPlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class RelicRecommendationDto
{
    public int RelicId { get; set; }
    public string Name { get; set; } = null!;
    public string Era { get; set; } = null!;
    public int Score { get; set; }
    public int BenefitingPlayerCount { get; set; }
    public List<RelicRecommendationOwnerDto> Owners { get; set; } = new();
    public List<RelicRecommendationRewardDto> UsefulRewards { get; set; } = new();
}

public class RelicRecommendationOwnerDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = null!;
    public int TotalCount { get; set; }
    public RelicRefinementCountsDto Refinements { get; set; } = new();
}

public class RelicRefinementCountsDto
{
    public int Intact { get; set; }
    public int Exceptional { get; set; }
    public int Flawless { get; set; }
    public int Radiant { get; set; }
}

public class RelicRecommendationRewardDto
{
    public string? ItemName { get; set; }
    public string ItemUniqueName { get; set; } = null!;
    public string Rarity { get; set; } = null!;
    public int NeedPoints { get; set; }
    public List<RelicRecommendationNeedDto> Players { get; set; } = new();
}

public class RelicRecommendationNeedDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = null!;
    public int MissingCount { get; set; }
    public List<string> RequiredFor { get; set; } = new();
}
