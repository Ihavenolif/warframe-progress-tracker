using rest_api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using rest_api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationService();

builder.Services.AddSingleton<ConfigurationService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<WarframeTrackerDbContext>(options => options.UseNpgsql(config.DataSource));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token with Bearer prefix, e.g., 'Bearer {your token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    config.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IMasteryService, MasteryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IWarframePublicExportService, WarframePublicExportService>();
builder.Services.AddScoped<IClanService, ClanService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.WithOrigins("https://www.localhost.me:8080")
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    options.AddPolicy("ProdPolicy", policy =>
    {
        policy.WithOrigins("https://" + config.OriginUrl)
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(config.GetJwtKey())
    };
});

if (config.AppEnvironment == "DEVELOPMENT")
{
    // Need self-signed https only for dev. For prod, nginx handles this.
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5224, listenOptions =>
        {
            listenOptions.UseHttps("../https-setup/localhost-me.pfx", "");
        });
    });
}

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(config.CorsPolicy);
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();


app.Run();
