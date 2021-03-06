using System;
using System.Data.SqlClient;

namespace Veff.Internal
{
    public class VeffSqlConnectionFactory : IVeffSqlConnectionFactory, IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection? _connection;

        public VeffSqlConnectionFactory(
            string connectionString,
            TimeSpan? cacheExpiry = null)
        {
            CacheExpiry = cacheExpiry ?? TimeSpan.FromSeconds(60);
            _connectionString = connectionString;
        }

        public SqlConnection UseConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            _connection = connection;

            return _connection;
        }

        public TimeSpan CacheExpiry { get; internal set; }

        public void Dispose()
        {
            _connection?.Close();
        }
    }
}