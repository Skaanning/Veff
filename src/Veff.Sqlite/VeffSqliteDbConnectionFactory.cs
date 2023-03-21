using System.Data.SQLite;
using Veff.Persistence;

namespace Veff.Sqlite;

internal class VeffSqliteDbConnectionFactory : IVeffDbConnectionFactory
{
    public TimeSpan CacheExpiry { get; set; }
    private readonly string _connectionString;

    internal VeffSqliteDbConnectionFactory(
        string connectionString,
        TimeSpan? cacheExpiry = null)
    {
        CacheExpiry = cacheExpiry ?? TimeSpan.FromSeconds(60);
        _connectionString = connectionString;
    }

    public IVeffDbConnection UseConnection()
    {
        var connection = new VeffSqliteConnection(new SQLiteConnection (_connectionString));

        return new VeffDbConnection(connection, this);
    }
}