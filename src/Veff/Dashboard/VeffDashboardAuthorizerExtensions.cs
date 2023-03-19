using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Veff.Extensions;

namespace Veff.Dashboard;

public static class VeffDashboardAuthorizerExtensions
{
    public static async Task<bool> IsAuthorized(
        this IEnumerable<IVeffDashboardAuthorizer> authorizers,
        HttpContext context)
    {
        var auths = authorizers.ToArray();
        if (auths.Length == 0)
            return true;
            
        var authorized = await auths.AnyAsync(async a => await a.IsAuthorized(context));

        if (authorized)
            return true;

        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("unauthorized");

        return false;
    }
}