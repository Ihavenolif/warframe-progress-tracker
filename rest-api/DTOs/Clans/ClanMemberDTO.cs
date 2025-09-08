namespace rest_api.DTOs.Clans;

public class ClanMemberDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public int MasteryRank { get; set; }
    public bool IsLeader { get; set; }
}