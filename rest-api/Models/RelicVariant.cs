namespace rest_api.Models;

public class RelicVariant
{
    public string UniqueName { get; set; } = null!;

    public int RelicId { get; set; }

    public RelicRefinement Refinement { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Relic Relic { get; set; } = null!;
}
