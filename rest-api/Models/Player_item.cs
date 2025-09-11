using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Player_item
{
    public string unique_name { get; set; } = null!;

    public int player_id { get; set; }

    public int item_count { get; set; }

    public virtual Player player { get; set; } = null!;

    public virtual Item unique_nameNavigation { get; set; } = null!;
}
