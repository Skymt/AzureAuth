using AzureAuth.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using System;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
namespace AzureAuth.DeveloperAuthorizer;

internal class DevJWTFunction
{
    readonly JWTManager jwtManager;
    public DevJWTFunction(JWTManager jwtManager) => this.jwtManager = jwtManager;

    [FunctionName("DevJWT"), SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Function trigger")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "DevJWT/{name}")] HttpRequest req,
        string name)
    {
        if (string.IsNullOrEmpty(name)) return new NotFoundResult();

        var claims = new Claim[] 
        {
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Role, "Developer")
        };
        return new OkObjectResult(jwtManager.Generate(claims, TimeSpan.FromMinutes(60)));
    }

}
