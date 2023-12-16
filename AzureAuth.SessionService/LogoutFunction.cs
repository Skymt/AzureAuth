using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static AzureAuth.Core.JWTManager;

namespace AzureAuth.SessionService;

internal class LogoutFunction
{
    readonly ClaimsRepository claimsRepository;
    public LogoutFunction(ClaimsRepository claimsRepository)
    {
        this.claimsRepository = claimsRepository;
    }

    [FunctionName("Logout")]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Logout")] HttpRequest req,
    ILogger log)
    {
        string securityToken = req.Cookies[SecurityTokenName] ?? req.Headers[SecurityHeaderName];
        if (Guid.TryParse(securityToken, out var token))
        {
            var _ = await claimsRepository.Delete(token);
            log.LogInformation("Logged out {token}", token);
        }
        req.HttpContext.Response.Cookies.Append(SecurityTokenName, string.Empty, new CookieOptions 
        { 
            Expires = DateTimeOffset.Now,
            HttpOnly = true,
            Secure = true,
            Path = req.Host.ToString()
        });
        return new OkResult();
    }
}
