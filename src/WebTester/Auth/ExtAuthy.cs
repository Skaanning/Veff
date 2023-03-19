using Veff.ExternalApi;

namespace WebTester.Auth;

public class ExtAuthy : IVeffExternalApiAuthorizer
{
    private readonly EmailFeatures _emailFeatures;

    public ExtAuthy(EmailFeatures emailFeatures)
    {
        _emailFeatures = emailFeatures;
    }
    
    public Task<bool> IsAuthorized(HttpContext context)
    {
        
        return Task.FromResult(_emailFeatures.SendSpamMails.IsDisabled);
    }
}

public class ExtAuthy2 : IVeffExternalApiAuthorizer
{
    public Task<bool> IsAuthorized(HttpContext context)
    {
        return Task.FromResult(true);
    }
}