using System.Data.SqlClient;

namespace Veff
{
    public interface IVeffSqlConnectionFactory
    {
        SqlConnection UseConnection();
    }
}