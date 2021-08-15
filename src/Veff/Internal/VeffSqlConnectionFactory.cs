using System;
using System.Data.SqlClient;

namespace Veff
{
    public class VeffSqlConnectionFactory : IVeffSqlConnectionFactory, IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;

        public VeffSqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection UseConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            _connection = connection;

            return _connection;
        }

        public void Dispose()
        {
            _connection?.Close();
        }
    }
}