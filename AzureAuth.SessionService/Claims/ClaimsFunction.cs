using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureAuth.SessionService;

public class ClaimsFunction
{
    // Repository for managing claims
    readonly ClaimsRepository claimsRepository;

    // Constructor for ClaimsFunction
    public ClaimsFunction(ClaimsRepository claimsRepository) => this.claimsRepository = claimsRepository;

    /// <summary>
    /// Function to handle claims
    /// </summary>
    /// <param name="req">The HTTP request</param>
    /// <param name="token">The token</param>
    /// <returns>The IActionResult</returns>
    [FunctionName("Claims")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "get", "post", "put", "delete", Route = "claims/{token?}")] HttpRequest req,
        Guid? token)
    {
        // Handle POST and PUT requests
        if (req.Method == HttpMethods.Post || req.Method == HttpMethods.Put)
            return await SaveClaims(req);

        // Handle GET and DELETE requests with token
        if (token.HasValue)
        {
            if (req.Method == HttpMethods.Get)
                return await GetClaims(token.Value);
            else if (req.Method == HttpMethods.Delete)
                return await DeleteClaims(token.Value);
        }
        // Handle GET request without token
        else if (req.Method == HttpMethods.Get)
            return await GetAllClaims();

        // Return NotFoundResult if no conditions met
        return new NotFoundResult();
    }

    // Method to save claims
    async Task<IActionResult> SaveClaims(HttpRequest req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var claims = JsonConvert.DeserializeObject<ClaimsEntity>(body);
        if (claims != null)
        {
            await claimsRepository.Set(claims);
            return new OkObjectResult(claims);
        }
        return new NotFoundResult();
    }

    // Method to delete claims
    async Task<IActionResult> DeleteClaims(Guid token)
    {
        await claimsRepository.Delete(token);
        return new OkResult();
    }

    // Method to get claims
    async Task<IActionResult> GetClaims(Guid token)
    {
        var claims = await claimsRepository.Fetch(token);
        if (claims != null)
        {
            return new OkObjectResult(claims);
        }
        return new NotFoundResult();
    }

    // Method to get all claims
    async Task<IActionResult> GetAllClaims()
    {
        var claims = await claimsRepository.FetchUnrecycled();
        if (claims != null)
        {
            return new OkObjectResult(claims);
        }
        return new NotFoundResult();
    }
}