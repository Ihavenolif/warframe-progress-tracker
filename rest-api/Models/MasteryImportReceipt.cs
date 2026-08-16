namespace rest_api.Models;

public class MasteryImportReceipt
{
    public int Id { get; set; }

    public int PlayerId { get; set; }

    public DateTime ImportedAt { get; set; }

    public int ResultingMasteryRank { get; set; }

    public int ResultingTotalMasteryXp { get; set; }

    public bool Changed { get; set; }

    public string? SourceVersion { get; set; }

    public int ProcessedCount { get; set; }

    public int SkippedCount { get; set; }

    public virtual Player Player { get; set; } = null!;
}
