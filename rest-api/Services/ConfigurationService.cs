using System.Text;
using DotNetEnv;
using Npgsql;
using rest_api.Models;

namespace rest_api.Services;

public class ConfigurationService
{
    public ConfigurationService()
    {
        Env.Load("../.env");
        CorsPolicy = Environment.GetEnvironmentVariable("CORS_POLICY") ?? "AllowAll";
        SecureCookies = Environment.GetEnvironmentVariable("SECURE_COOKIES") == "true";

        dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");
        dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        this.AppEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "DEVELOPMENT";
        this.OriginUrl = Environment.GetEnvironmentVariable("ORIGIN_URL") ?? "";
        this.DataUpdateServerUrl = Environment.GetEnvironmentVariable("DATA_UPDATE_SERVER_URL") ?? "localhost";

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GetConnectionString());
        dataSourceBuilder.EnableDynamicJson();
        dataSourceBuilder.MapEnum<InvitationStatus>("invitation_status");
        DataSource = dataSourceBuilder.Build();
    }

    public readonly string CorsPolicy;
    public readonly bool SecureCookies;
    public readonly NpgsqlDataSource DataSource;
    public readonly string AppEnvironment;
    public readonly string OriginUrl;
    public readonly string DataUpdateServerUrl;

    private string? dbHost;
    private string? dbName;
    private string? dbUser;
    private string? dbPassword;

    private string GetConnectionString()
    {
        if (dbHost == null || dbName == null || dbUser == null || dbPassword == null)
        {
            throw new ArgumentNullException("Missing environment variables");
        }

        var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword};Pooling=true;Maximum Pool Size=10;Connection Idle Lifetime=30;";

        return connectionString;

    }

    public byte[] GetJwtKey()
    {

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

        if (jwtKey == null)
        {
            throw new ArgumentNullException("JWT_KEY environment variable is missing");
        }

        var key = Encoding.ASCII.GetBytes(jwtKey);

        return key;

    }
}