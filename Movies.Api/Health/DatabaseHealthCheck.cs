using System.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnectionFactory dbConnection, ILogger<DatabaseHealthCheck> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            await _dbConnection.CreateConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            const string erroMessage = "Database is unhealthy";
            _logger.LogError(erroMessage);
            return HealthCheckResult.Unhealthy(e.Message);
        } 
    }
}