using System.Data.SqlClient;

namespace Veff.Internal
{
    public interface IVeffSqlConnectionFactory
    {
        SqlConnection UseConnection();
    }
}