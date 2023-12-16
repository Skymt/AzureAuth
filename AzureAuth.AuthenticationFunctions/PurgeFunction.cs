using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace AzureAuth.SessionService;

internal class PurgeFunction
{
    readonly ClaimsRepository claimsRepository;
    public PurgeFunction(ClaimsRepository claimsRepository)
    {
        this.claimsRepository = claimsRepository;
    }

    [FunctionName("Purge"), SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Function trigger parameter.")]
    public async Task Run([TimerTrigger("55 23 * * *")]TimerInfo myTimer, ILogger log)
    {
        var purgeCount = await claimsRepository.Purge();
        log.LogInformation($"Purged {purgeCount} expired claims.");
    }
}
