using AzureAuth.SessionService.AuthHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static AzureAuth.Core.JWTManager;

namespace AzureAuth.SessionService;

internal class LoginFunction
{
    readonly UserAuthHandler userHandler;
    readonly ServiceAuthHandler serviceHandler;

    public LoginFunction(UserAuthHandler userHandler, ServiceAuthHandler serviceHandler)
        => (this.userHandler, this.serviceHandler) = (userHandler, serviceHandler);

    [FunctionName("Login")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, 
        ILogger log)
    {
        // Check if request is from a service or from a user
        if(req.Headers.ContainsKey(SecurityHeaderName))
        {
            log.LogInformation("Request is from a service");
            return await serviceHandler.AuthorizeService(req, log);
        }
        log.LogInformation("Request is from a user");
        return await userHandler.AuthorizeUser(req, log);
    }
}
