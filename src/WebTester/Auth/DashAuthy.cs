using Veff.Dashboard;

namespace WebTester.Auth;

public class Authy : IVeffDashboardAuthorizer
{
    private readonly EmailFeatures _emailFeatures;

    public Authy(EmailFeatures emailFeatures)
    {
        _emailFeatures = emailFeatures;
    }
    
    public Task<bool> IsAuthorized(HttpContext context)
    {
        return Task.FromResult( _emailFeatures.SendSpamMails.IsDisabled);
    }
}

public class DashAuthy2 : IVeffDashboardAuthorizer
{
    public Task<bool> IsAuthorized(HttpContext context)
    {
        return Task.FromResult(true);
    }
}