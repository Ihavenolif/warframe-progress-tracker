using rest_api.Models;

namespace rest_api.DTO;

public class UserInfoDTO
{
    public UserInfoDTO(Registered_user user)
    {
        this.player_id = user.player_id;
        this.username = user.username;
        this.playerName = user.player == null ? null : user.player.username;
    }

    public int? player_id { get; set; }
    public string? username { get; set; }
    public string? playerName { get; set; }

}