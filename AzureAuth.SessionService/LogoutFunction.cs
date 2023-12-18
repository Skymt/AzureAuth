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

    /// <summary>
    /// Logging out deletes the stored claims associated with the 
    /// auth id token. It also clears the auth id token from the 
    /// browser cookie.
    /// </summary>
    /// <param name="req">The request object</param>
    /// <param name="log">The target for informative information</param>
    /// <returns>This endpoint only makes sense for users. Services
    /// will just refresh their JWT using <see cref="LoginFunction"/> 
    /// when it expires.</returns>
    [FunctionName("Logout")]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Logout")] HttpRequest req,
    ILogger log)
    {
        string authId = req.Cookies[AuthCookieName];
        if (Guid.TryParse(authId, out var token))
        {
            var _ = await claimsRepository.Delete(token);
            log.LogInformation("Logged out {token}", token);
        }
        req.HttpContext.Response.Cookies.Append(AuthCookieName, string.Empty, new CookieOptions
        {
            Expires = DateTimeOffset.Now,
            HttpOnly = true,
            Secure = true,
            Path = req.Host.ToString()
        });
        return new OkResult();
    }
}
