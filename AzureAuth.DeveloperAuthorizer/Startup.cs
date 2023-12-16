using AzureAuth.Core;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureAuth.DeveloperAuthorizer.Startup))]
namespace AzureAuth.DeveloperAuthorizer;

internal class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddJWTManager();
    }
}
