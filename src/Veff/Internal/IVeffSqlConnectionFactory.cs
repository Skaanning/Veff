using System.Data.SqlClient;

namespace Veff.Internal
{
    public interface IVeffSqlConnectionFactory
    {
        SqlConnection UseConnection();
        int CacheExpiryInSeconds { get; }
    }
}