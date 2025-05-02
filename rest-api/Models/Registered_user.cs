using System;
using System.Collections.Generic;

namespace rest_api.Models;

public partial class Registered_user
{
    public Registered_user() { }

    public Registered_user(string username, string password)
    {
        this.username = username;
        this.password_hash = password;
    }

    public int id { get; set; }

    public int? player_id { get; set; }

    public string username { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public virtual Player? player { get; set; }
}
