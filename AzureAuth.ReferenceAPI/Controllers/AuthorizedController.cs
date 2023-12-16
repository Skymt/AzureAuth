﻿using AzureAuth.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AzureAuth.ReferenceAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AuthorizedController : ControllerBase
    {
        readonly JWTManager jwtManager;
        public AuthorizedController(JWTManager jwtManager) => this.jwtManager = jwtManager;

        [HttpGet, Authorize]
        public string? Get()
        {
            return HttpContext.User.Identity?.Name;
        }

        [HttpPost, Authorize]
        public string? Post()
        {
            var claims = HttpContext.User.Claims.ToList();

            if (!claims.Any(c => c.Type == "Status"))
            {
                claims.Add(new Claim("Status", "This guy gets around!"));
                var newJWT = jwtManager.Generate(claims, TimeSpan.FromMinutes(15));
                HttpContext.Response.Headers.Authorization = "Bearer " + newJWT;
            }
            return HttpContext.User.Identity?.Name;
        }
    }
}
