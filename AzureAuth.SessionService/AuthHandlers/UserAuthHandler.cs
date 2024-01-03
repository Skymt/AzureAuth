using AzureAuth.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;
using static AzureAuth.Core.JWTManager;

#nullable enable
namespace AzureAuth.SessionService.AuthHandlers
{
    internal class UserAuthHandler
    {
        readonly ClaimsRepository claimsRepository;
        readonly JWTManager jwtManager;
        public UserAuthHandler(ClaimsRepository claimsRepository, JWTManager jwtManager)
            => (this.claimsRepository, this.jwtManager) = (claimsRepository, jwtManager);

        public async Task<IActionResult> AuthorizeUser(HttpRequest req, ILogger log)
        {
            string? authId = req.Cookies[AuthCookieName];
            bool hasAuthId = Guid.TryParse(authId, out var refreshToken);

            var (hasValidJWT, jwt) = await HasValidJWT(req);
            ClaimsEntity? userClaims = null;

            if (!hasAuthId && hasValidJWT)
            {
                userClaims = defaultClaims();
                userClaims.SetClaims(jwt!.ClaimsIdentity.Claims);
                log.LogInformation("Created new user session.");
            }
            else if (hasAuthId)
            {
                var retiredClaims = await claimsRepository.Get(refreshToken);
                if (retiredClaims != null)
                {
                    retiredClaims.Token = Guid.NewGuid();
                    log.LogInformation("Refreshed user session {refreshToken}.", refreshToken);
                }

                userClaims = retiredClaims ?? defaultClaims();
                if (hasValidJWT)
                {
                    if (userClaims.Claims == null)
                    {
                        log.LogInformation("Revived user session {refreshToken}", refreshToken);
                    }
                    userClaims.Claims = jwt!.ClaimsIdentity.Claims.ToArray();
                }
            }

            if (userClaims?.Claims == null)
            {
                log.LogInformation("User authorization failed.");
                return new UnauthorizedResult();
            }

            await claimsRepository.Set(userClaims);
            var newJwt = jwtManager.Generate(userClaims.Claims, userClaims.Duration, "Users");
            
            req.HttpContext.Response.Cookies.Append(AuthCookieName, $"{userClaims.Token}", defaultCookie());
            log.LogInformation("Authorized {refreshToken} -> {Token}", refreshToken, userClaims.Token);
            return new OkObjectResult(new 
            { 
                token = newJwt, 
                refreshAt = (userClaims.Duration - TimeSpan.FromSeconds(30)).TotalMilliseconds 
            });

            CookieOptions defaultCookie() => new() { Expires = DateTimeOffset.Now.AddDays(7), HttpOnly = true, Secure = true, Path = req.Host.ToString() };
            static ClaimsEntity defaultClaims() => new() { Expires = DateTimeOffset.Now.AddDays(7), Duration = TimeSpan.FromMinutes(15), Recycled = true };
        }

        async Task<(bool, TokenValidationResult?)> HasValidJWT(HttpRequest req)
        {
            if (req.Headers.TryGetValue("Authorization", out var jwt))
            {
                var validation = await jwtManager.Validate(jwt[0][7..]);
                return (validation.IsValid, validation);
            }
            return (false, null);
        }
    }
}
