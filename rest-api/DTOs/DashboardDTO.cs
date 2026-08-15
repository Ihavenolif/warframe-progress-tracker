namespace rest_api.DTO;

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
