using Hangfire.Dashboard;

namespace Coffee.Api.Hangfire;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // En producción, implementar autenticación real
        if (context.GetHttpContext().Request.Headers.ContainsKey("X-Hangfire-Token"))
        {
            return context.GetHttpContext().Request.Headers["X-Hangfire-Token"] == "YourSecretToken";
        }
        
        // Permitir en desarrollo
        if (context.GetHttpContext().Request.Host.ToString().Contains("localhost"))
        {
            return true;
        }

        return false;
    }
}