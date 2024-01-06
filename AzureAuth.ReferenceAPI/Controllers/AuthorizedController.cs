using AzureAuth.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public object[] Patch()
    {
        // The client in index.html will check response headers and update
        // the JWT if it finds one by a different issuer than the SessionService.
        var claims = HttpContext.User.Claims.ToList();

        claims.Add(new Claim("Status", "This guy gets around!"));
        claims.Add(new Claim(ClaimTypes.Role, "AlgorithmEvaluator"));
        // (The claim "aud", audience, is set when the JWT is generated,
        // and will not be included in the response.)

        jwtManager.Generate(claims, TimeSpan.FromMinutes(5), response: HttpContext.Response);
        return claims.Select(c => new { c.Type, c.Value }).ToArray();
    }
}
