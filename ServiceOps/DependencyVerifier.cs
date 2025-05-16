using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Confluent.Kafka;

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
    }
}