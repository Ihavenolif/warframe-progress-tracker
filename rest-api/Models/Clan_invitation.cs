using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Clan_invitation
{
    public int id { get; set; }

    public int clan_id { get; set; }

    public int player_id { get; set; }

    public virtual Clan clan { get; set; } = null!;

    public virtual Player player { get; set; } = null!;
}
