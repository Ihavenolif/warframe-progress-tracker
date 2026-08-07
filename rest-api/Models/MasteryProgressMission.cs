namespace rest_api.Models;

public partial class MasteryProgressMission
{
    public int Id { get; set; }

    public int MasteryProgressEntryId { get; set; }

    public string UniqueName { get; set; } = null!;

    public string? Name { get; set; }

    public string? Planet { get; set; }

    public int PreviousCompletionCount { get; set; }

    public int CurrentCompletionCount { get; set; }

    public bool PreviousSPComplete { get; set; }

    public bool CurrentSPComplete { get; set; }

    public int MasteryXpGained { get; set; }

    public virtual MasteryProgressEntry ProgressEntry { get; set; } = null!;

    public virtual Mission Mission { get; set; } = null!;
}
