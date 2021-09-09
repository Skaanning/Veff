using Veff.Internal;

namespace Veff.Flags
{
    public abstract class Flag
    {
        protected readonly IVeffSqlConnectionFactory VeffSqlConnectionFactory;

        protected Flag(
            IVeffSqlConnectionFactory veffSqlConnectionFactory)
        {
            VeffSqlConnectionFactory = veffSqlConnectionFactory;
        }
    }
}