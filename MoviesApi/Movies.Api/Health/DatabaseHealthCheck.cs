using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;

public class DatabaseHealthCheck(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<DatabaseHealthCheck> logger)
    : IHealthCheck
{
    public const string Name = "Database";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        try
        {
            _ = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            logger.LogError("Database is unhealthy {ex}", ex);
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }
}