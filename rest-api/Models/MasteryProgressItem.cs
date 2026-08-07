namespace rest_api.Models;

using rest_api.Services;

public partial class MasteryProgressItem
{
    public int Id { get; set; }

    public int MasteryProgressEntryId { get; set; }

    public string? Name { get; set; }

    public int PreviousXp { get; set; }

    public int CurrentXp { get; set; }

    public int MasteryXpGained { get; set; }

    public virtual MasteryProgressEntry ProgressEntry { get; set; } = null!;

    public virtual Item Item { get; set; } = null!;

    public int PreviousRank => MasteryService.GetItemRank(Item.xp_required ?? throw new InvalidOperationException("Unknown item xp_required value"), PreviousXp);

    public int CurrentRank => MasteryService.GetItemRank(Item.xp_required ?? throw new InvalidOperationException("Unknown item xp_required value"), CurrentXp);
}
