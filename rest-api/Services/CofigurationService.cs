using System.Text;
using DotNetEnv;

namespace rest_api.Services;

public class ConfigurationService
{
    public ConfigurationService()
    {
        Env.Load("../.env");
        CorsPolicy = Environment.GetEnvironmentVariable("CORS_POLICY") ?? "AllowAll";
    }

    public readonly string CorsPolicy;

    public string GetConnectionString()
    {
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASS");

        if (dbHost == null || dbName == null || dbUser == null || dbPassword == null)
        {
            throw new ArgumentNullException("Missing environment variables");
        }

        var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}";

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