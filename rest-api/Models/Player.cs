using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Player
{
    public int id { get; set; }

    public string username { get; set; } = null!;

    public int mastery_rank { get; set; }

    public virtual ICollection<Clan_invitation> clan_invitations { get; set; } = new List<Clan_invitation>();

    public virtual ICollection<Clan> clans { get; set; } = new List<Clan>();

    public virtual ICollection<Player_item> player_items { get; set; } = new List<Player_item>();

    public virtual ICollection<Player_items_mastery> player_items_masteries { get; set; } = new List<Player_items_mastery>();

    public virtual ICollection<Registered_user> registered_users { get; set; } = new List<Registered_user>();

    public virtual ICollection<Clan> clansNavigation { get; set; } = new List<Clan>();
}
