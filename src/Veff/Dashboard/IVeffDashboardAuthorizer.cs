using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Veff.Dashboard;

public interface IVeffDashboardAuthorizer
{
    Task<bool> IsAuthorized(
        HttpContext context);
}