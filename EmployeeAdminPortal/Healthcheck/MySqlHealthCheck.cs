using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace Employee.API.Healthcheck
{
    public class MySqlHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public MySqlHealthCheck(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection =
                    new MySqlConnection(_connectionString);

                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";

                await command.ExecuteScalarAsync(cancellationToken);

                return HealthCheckResult.Healthy(
                    "MySQL database connection is functional.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: "MySQL database check failed.",
                    exception: ex);
            }
        }
    }
}
