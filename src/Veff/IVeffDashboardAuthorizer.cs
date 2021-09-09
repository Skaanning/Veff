using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Veff
{
    public interface IVeffDashboardAuthorizer
    {
        Task<bool> IsAuthorized(
            HttpContext context);
    }
}