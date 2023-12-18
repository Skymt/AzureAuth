using AzureAuth.Core;
using AzureAuth.SessionService.AuthHandlers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureAuth.SessionService.Startup))]
namespace AzureAuth.SessionService;

internal class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<ClaimsRepository>();
        builder.Services.AddSingleton<UserAuthHandler>();
        builder.Services.AddSingleton<ServiceAuthHandler>();
        builder.Services.AddJWTManager();
    }
}
