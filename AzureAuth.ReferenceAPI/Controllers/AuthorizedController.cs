using AzureAuth.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Security.Claims;

namespace AzureAuth.ReferenceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthorizedController : ControllerBase
{
    readonly JWTManager jwtManager;
    public AuthorizedController(JWTManager jwtManager) => this.jwtManager = jwtManager;

    [HttpGet, Authorize]
    public string? Get() => HttpContext.User.Identity?.Name;

    [HttpPost, Authorize]
    public object[] Post()
    {
        var claims = HttpContext.User.Claims.ToList();
        return claims.Select(c => new { c.Type, c.Value }).ToArray();
    }

    [HttpPatch, Authorize]
    public string? Patch()
    {
        // The client in index.html will check response headers and update
        // the JWT if it finds one by a different issuer than the SessionService.
        var claims = HttpContext.User.Claims.ToList();

        if (!claims.Any(c => c.Type == "Status"))
        {
            claims.Add(new Claim("Status", "This guy gets around!"));
            claims.Add(new Claim(ClaimTypes.Role, "AlgorithmEvaluator"));
            var newJWT = jwtManager.Generate(claims, TimeSpan.FromMinutes(15));
            HttpContext.Response.Headers.Authorization = "Bearer " + newJWT;
        }
        return HttpContext.User.Identity?.Name;
    }
}
