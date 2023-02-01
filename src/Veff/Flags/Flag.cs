using Veff.Internal;

namespace Veff.Flags
{
    public abstract class Flag
    {
        public readonly IVeffDbConnectionFactory VeffDbConnectionFactory;

        protected Flag(
            IVeffDbConnectionFactory veffDbConnectionFactory)
        {
            VeffDbConnectionFactory = veffDbConnectionFactory;
        }
    }
}