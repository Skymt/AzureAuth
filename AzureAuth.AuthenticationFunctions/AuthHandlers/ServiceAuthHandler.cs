using AzureAuth.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static AzureAuth.Core.JWTManager;

#nullable enable
namespace AzureAuth.SessionService.AuthHandlers
{
    internal class ServiceAuthHandler
    {
        readonly ClaimsRepository claimsRepository;
        readonly JWTManager jwtManager;
        public ServiceAuthHandler(ClaimsRepository claimsRepository, JWTManager jwtManager)
            => (this.claimsRepository, this.jwtManager) = (claimsRepository, jwtManager);
        public async Task<IActionResult> AuthorizeService(HttpRequest req, ILogger log)
        {
            string? securityToken = req.Headers[SecurityHeaderName];
            if (Guid.TryParse(securityToken, out var token))
            {
                log.LogInformation("Service token {token}", token);
                var credentials = await claimsRepository.Get(token);
                if (credentials != null)
                {
                    log.LogInformation("Service authorized");
                    var jwt = jwtManager.Generate(credentials.Claims, credentials.Duration, "Services");
                    req.HttpContext.Response.Headers.Authorization = "Bearer " + jwt;
                    return new OkResult();
                }
            }

            log.LogWarning("Serive token unreadable.");
            return new UnauthorizedResult();
        }
    }
}
