using AzureAuth.SessionService.AuthHandlers;
using AzureAuth.Core;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;

[assembly: FunctionsStartup(typeof(AzureAuth.SessionService.Startup))]
namespace AzureAuth.SessionService;

internal class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton(CloudStorageAccount.DevelopmentStorageAccount);
        builder.Services.AddSingleton<ClaimsRepository>();

        builder.Services.AddSingleton<UserAuthHandler>();
        builder.Services.AddSingleton<ServiceAuthHandler>();
        builder.Services.AddJWTManager();
    }
}
