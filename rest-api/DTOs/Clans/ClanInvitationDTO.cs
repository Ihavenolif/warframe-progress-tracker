namespace rest_api.DTOs.Clans;

public class ClanInvitationDTO
{
    public int Id { get; set; }
    public string ClanName { get; set; } = null!;
    public string PlayerName { get; set; } = null!;
}