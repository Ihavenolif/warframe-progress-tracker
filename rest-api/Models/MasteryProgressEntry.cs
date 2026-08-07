using System;

namespace rest_api.Models;

public partial class MasteryProgressEntry
{
    public int Id { get; set; }

    public int PlayerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int PreviousTotalMasteryXp { get; set; }

    public int CurrentTotalMasteryXp { get; set; }

    public int MasteryXpGained { get; set; }

    public virtual Player Player { get; set; } = null!;

    public virtual ICollection<MasteryProgressItem> LeveledItems { get; set; } = new List<MasteryProgressItem>();

    public virtual ICollection<MasteryProgressMission> Missions { get; set; } = new List<MasteryProgressMission>();
}
