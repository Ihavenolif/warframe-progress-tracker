namespace rest_api.DTOs.Clans;

public class ClanDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? LeaderName { get; set; }
}