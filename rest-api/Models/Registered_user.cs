using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Registered_user
{
    public int id { get; set; }

    public int? player_id { get; set; }

    public string username { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public string salt { get; set; } = null!;

    public virtual Player? player { get; set; }
}
