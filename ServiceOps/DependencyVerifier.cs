using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Confluent.Kafka;
using System.Net.Http; // Added for HttpClient

public static class DependencyVerifier
{
    public static async Task VerifyDependenciesAsync(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("StartupChecks");
        // Database check
        var connStr = app.Configuration.GetConnectionString("DefaultConnection");
        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            logger.LogInformation("Database connection successful.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to connect to database.");
            throw;
        }

        // Auth0 check
        var auth0Domain = app.Configuration["Auth0:Domain"];
        if (string.IsNullOrEmpty(auth0Domain) || auth0Domain.Contains("YOUR_AUTH0_DOMAIN"))
        {
            logger.LogWarning("Auth0 domain is not configured or using placeholder. Skipping Auth0 check.");
        }
        else
        {
            try
            {
                using var httpClient = new HttpClient();
                var wellKnownUrl = $"https://{auth0Domain}/.well-known/jwks.json";
                var response = await httpClient.GetAsync(wellKnownUrl);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Auth0 connection successful (checked .well-known/jwks.json).");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to connect to Auth0 (could not retrieve .well-known/jwks.json).");
                // Decide if this should be a critical failure that stops startup
                // For now, logging as critical but not throwing to allow startup if Auth0 is temporarily unavailable
                // throw; 
            }
        }
        // Kafka check
        var kafkaConfig = new ProducerConfig { BootstrapServers = app.Configuration["Kafka:BootstrapServers"] };
        try
        {
            using var adminClient = new AdminClientBuilder(kafkaConfig).Build();
            var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            logger.LogInformation("Kafka connection successful.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to connect to Kafka.");
            throw;
        }

        // Auth0 check
        var auth0Domain = app.Configuration["Auth0:Domain"];
        if (string.IsNullOrEmpty(auth0Domain) || auth0Domain.Contains("YOUR_AUTH0_DOMAIN"))
        {
            logger.LogWarning("Auth0 domain is not configured or using placeholder. Skipping Auth0 check.");
        }
        else
        {
            try
            {
                using var httpClient = new HttpClient();
                var wellKnownUrl = $"https://{auth0Domain}/.well-known/jwks.json";
                var response = await httpClient.GetAsync(wellKnownUrl);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Auth0 connection successful (checked .well-known/jwks.json).");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to connect to Auth0 (could not retrieve .well-known/jwks.json).");
                // Decide if this should be a critical failure that stops startup
                // For now, logging as critical but not throwing to allow startup if Auth0 is temporarily unavailable
                // throw; 
            }
        }
    }
}