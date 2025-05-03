using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

    [JsonIgnore]
    public string password_hash { get; set; } = null!;

    [JsonIgnore]
    public virtual Player? player { get; set; }
}
