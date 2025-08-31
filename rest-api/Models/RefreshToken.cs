namespace rest_api.Models;

public partial class RefreshToken
{
    public RefreshToken() { }
    public RefreshToken(string token, Registered_user user, string? ip)
    {
        this.Token = token;
        this.User = user;
        this.UserId = user.id;
        this.IssuedByIp = ip;
    }

    public string Token { get; set; } = null!;

    public DateTime Issued { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

    public DateTime Expires { get; set; } = DateTime.SpecifyKind(DateTime.Now.AddDays(7), DateTimeKind.Unspecified);

    public bool Revoked { get; set; } = false;

    public string? IssuedByIp { get; set; } = null;

    public bool IsExpired => DateTime.UtcNow >= Expires;

    public Registered_user? User { get; set; }

    public int UserId { get; set; }
}