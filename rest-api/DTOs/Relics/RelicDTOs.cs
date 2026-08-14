namespace rest_api.DTOs.Relics;

public class RelicQueryDTO
{
    public string? Search { get; set; }
    public string? Era { get; set; }
    public string? Refinement { get; set; }
    public string Owned { get; set; } = "all";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = "name";
}

public class RelicPageDTO
{
    public List<RelicDTO> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class RelicDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Era { get; set; } = null!;
    public int TotalOwned { get; set; }
    public List<RelicVariantDTO> Variants { get; set; } = new();
    public List<RelicRewardDTO> Rewards { get; set; } = new();
}

public class RelicVariantDTO
{
    public string UniqueName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Refinement { get; set; } = null!;
    public int Quantity { get; set; }
}

public class RelicRewardDTO
{
    public string? ItemName { get; set; }
    public string UniqueName { get; set; } = null!;
    public string Rarity { get; set; } = null!;
    public int ItemCount { get; set; }
}
