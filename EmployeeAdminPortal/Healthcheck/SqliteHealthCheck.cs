using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Employee.API.Healthcheck
{
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        public SqliteHealthCheck(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
        {
            try
            {
                // Open a connection to the SQLite database file
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Execute a simple command to ensure the file is readable/writeable
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                await command.ExecuteScalarAsync(cancellationToken);

                return HealthCheckResult.Healthy("SQLite database connection is functional.");
            }
            catch (Exception ex)
            {
                // Return Unhealthy status along with the caught exception details
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: "SQLite database check failed.",
                    exception: ex);
            }
        }

    }
}