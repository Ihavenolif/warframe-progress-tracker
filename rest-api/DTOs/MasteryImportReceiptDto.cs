namespace rest_api.DTO;

public class MasteryImportReceiptDto
{
    public int Id { get; set; }

    public long ImportedAt { get; set; }

    public int ResultingMasteryRank { get; set; }

    public int ResultingTotalMasteryXp { get; set; }

    public bool Changed { get; set; }

    public string? SourceVersion { get; set; }

    public int ProcessedCount { get; set; }

    public int SkippedCount { get; set; }
}
