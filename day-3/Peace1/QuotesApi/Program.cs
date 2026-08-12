using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Extensions;
using QuotesApi.Middleware;
using QuotesApi.Repositories;
using QuotesApi.Services;
using QuotesApi.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddSingleton<IClock, QuotesApi.Services.SystemClock>();
builder.Services.AddTransient<IQuoteFormatter, QuoteFormatter>();

// --------------------------------------------------
// JWT configuration

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT key is not configured.");

var internalIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var internalAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

// --------------------------------------------------
// Entra ID configuration

var tenantId = builder.Configuration["Entra:TenantId"]
    ?? throw new InvalidOperationException(
        "Entra tenant ID is not configured.");

var entraAudience = builder.Configuration["Entra:Audience"]
    ?? throw new InvalidOperationException(
        "Entra audience is not configured.");

var entraAuthority =
    $"https://login.microsoftonline.com/{tenantId}/v2.0";

// --------------------------------------------------
// Authentication

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Smart";
        options.DefaultChallengeScheme = "Smart";
    })

    // Our own JWT - used by internal callers
    .AddJwtBearer("InternalJwt", options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = internalIssuer,
                ValidAudience = internalAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    })

    // Microsoft Entra ID JWT
    .AddJwtBearer("Entra", options =>
    {
        options.Authority = entraAuthority;
        options.Audience = entraAudience;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,

                ValidIssuer = entraAuthority,
                ValidAudience = entraAudience
            };
    })

    // Select the authentication scheme based on "iss"
    .AddPolicyScheme(
        "Smart",
        "Internal JWT or Entra JWT",
        options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                var authorization =
                    context.Request.Headers.Authorization
                        .ToString();

                if (!string.IsNullOrWhiteSpace(authorization) &&
                    authorization.StartsWith(
                        "Bearer ",
                        StringComparison.OrdinalIgnoreCase))
                {
                    var token = authorization["Bearer ".Length..]
                        .Trim();

                    var handler =
                        new JwtSecurityTokenHandler();

                    if (handler.CanReadToken(token))
                    {
                        var jwt = handler.ReadJwtToken(token);

                        if (jwt.Issuer == entraAuthority)
                        {
                            return "Entra";
                        }

                        if (jwt.Issuer == internalIssuer)
                        {
                            return "InternalJwt";
                        }
                    }
                }

                // Default for requests without a readable token.
                return "InternalJwt";
            };
        });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();

// --------------------------------------------------
// Database migrations

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<QuoteDbContext>();

    db.Database.Migrate();
}

// --------------------------------------------------
// Development test user

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<QuoteDbContext>();

    if (!db.Users.Any(u => u.Email == "test@example.com"))
    {
        db.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    "Test123!")
        });

        db.SaveChanges();
    }
}

app.MapGet("/", () => "Quotes API is running!");

app.MapQuoteEndpoints();
app.MapAuthEndpoints();
app.MapCollectionEndpoints();

app.Run();

public partial class Program { }