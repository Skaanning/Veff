using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Veff.ExternalApi;

public interface IVeffExternalApiAuthorizer
{
    Task<bool> IsAuthorized(
        HttpContext context);
}