namespace rest_api.Models;

public partial class RefreshToken
{
    public string Token { get; set; } = null!;

    public DateTime Expires { get; set; }

    public DateTime Issued { get; set; } = DateTime.Now;

    public bool Revoked { get; set; }

    public string IssuedByIp { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= Expires;

    public Registered_user? User { get; set; }

    public int UserId { get; set; }
}