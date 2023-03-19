using System.Data.SqlClient;
using Veff.Persistence;

namespace Veff.SqlServer;

internal class VeffSqlServerDbConnectionFactory : IVeffDbConnectionFactory
{
    public TimeSpan CacheExpiry { get; set; }
    private readonly string _connectionString;

    internal VeffSqlServerDbConnectionFactory(
        string connectionString,
        TimeSpan? cacheExpiry = null)
    {
        CacheExpiry = cacheExpiry ?? TimeSpan.FromSeconds(60);
        _connectionString = connectionString;
    }

    public IVeffDbConnection UseConnection()
    {
        var connection = new VeffSqlServerConnection(new SqlConnection(_connectionString));

        return new VeffDbConnection(connection, this);
    }
}