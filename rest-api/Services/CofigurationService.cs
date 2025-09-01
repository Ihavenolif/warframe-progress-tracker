using System.Text;
using DotNetEnv;
using Npgsql;

namespace rest_api.Services;

public class ConfigurationService
{
    public ConfigurationService()
    {
        Env.Load("../.env");
        CorsPolicy = Environment.GetEnvironmentVariable("CORS_POLICY") ?? "AllowAll";
        SecureCookies = Environment.GetEnvironmentVariable("SECURE_COOKIES") == "true";

        dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        dbName = Environment.GetEnvironmentVariable("DB_NAME");
        dbUser = Environment.GetEnvironmentVariable("DB_USER");
        dbPassword = Environment.GetEnvironmentVariable("DB_PASS");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(GetConnectionString());
        dataSourceBuilder.EnableDynamicJson();
        DataSource = dataSourceBuilder.Build();
    }

    public readonly string CorsPolicy;
    public readonly bool SecureCookies;
    public readonly NpgsqlDataSource DataSource;

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