namespace rest_api.DTO;

public class DashboardSummaryDto
{
    public int MasteryRank { get; set; }

    public int TotalMasteryXp { get; set; }

    public MasteryImportReceiptDto? LatestImport { get; set; }

    public int MasteryXpGained7Days { get; set; }

    public int MasteryXpGained30Days { get; set; }

    public DashboardItemCountsDto Items { get; set; } = new();

    public List<DashboardCategoryCompletionDto> Categories { get; set; } = [];

    public DashboardMissionTotalsDto Missions { get; set; } = new();

    public DashboardIntrinsicTotalsDto Intrinsics { get; set; } = new();
}

public class DashboardItemCountsDto
{
    public int Total { get; set; }

    public int Mastered { get; set; }

    public int Started { get; set; }

    public int Unowned { get; set; }

    public int CraftReady { get; set; }
}

public class DashboardCategoryCompletionDto
{
    public string Category { get; set; } = null!;

    public int Mastered { get; set; }

    public int Total { get; set; }
}

public class DashboardMissionTotalsDto
{
    public int NormalCompleted { get; set; }

    public int NormalTotal { get; set; }

    public int SteelPathCompleted { get; set; }

    public int SteelPathTotal { get; set; }
}

public class DashboardIntrinsicTotalsDto
{
    public int Railjack { get; set; }

    public int RailjackTotal { get; set; }

    public int Duviri { get; set; }

    public int DuviriTotal { get; set; }
}

public class DashboardProgressEntryDTO
{
    public int id { get; set; }

    public long createdAt { get; set; }

    public int previousTotalMasteryXp { get; set; }

    public int currentTotalMasteryXp { get; set; }

    public int masteryXpGained { get; set; }

    public List<DashboardProgressItemDTO> leveledItems { get; set; } = new List<DashboardProgressItemDTO>();

    public List<DashboardProgressMissionDTO> missions { get; set; } = new List<DashboardProgressMissionDTO>();
}

public class DashboardProgressItemDTO
{
    public string? uniqueName { get; set; }

    public string? name { get; set; }

    public int previousRank { get; set; }

    public int currentRank { get; set; }

    public int previousXp { get; set; }

    public int currentXp { get; set; }

    public int masteryXpGained { get; set; }
}

public class DashboardProgressMissionDTO
{
    public string? uniqueName { get; set; }

    public string? name { get; set; }

    public string? planet { get; set; }

    public bool completed { get; set; }

    public bool steelPathCompleted { get; set; }

    public int masteryXpGained { get; set; }
}

public class DashboardProgressDayDTO
{
    public DateTime date { get; set; }

    public int masteryXpGained { get; set; }
}
