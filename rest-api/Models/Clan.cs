using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Clan
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public int leader_id { get; set; }

    public virtual ICollection<Clan_invitation> clan_invitations { get; set; } = new List<Clan_invitation>();

    public virtual Player leader { get; set; } = null!;

    public virtual ICollection<Player> players { get; set; } = new List<Player>();
}
