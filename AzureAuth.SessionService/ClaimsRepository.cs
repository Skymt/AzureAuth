using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AzureAuth.SessionService;

public class ClaimsRepository
{
    readonly CloudTable Claims;

    public ClaimsRepository(IConfiguration configuration)
    {
        var account = CloudStorageAccount.Parse(configuration["AzureWebJobsStorage"]);
        var client = account.CreateCloudTableClient();

        Claims = client.GetTableReference("Claims");
        Claims.CreateIfNotExistsAsync().Wait();
    }

    public async Task<ClaimsEntity> Set(ClaimsEntity entity)
    {
        await Claims.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        return entity;
    }
    public async Task<ClaimsEntity> Get(Guid token)
    {
        var operation = await Claims.ExecuteAsync(TableOperation.Retrieve<ClaimsEntity>("Claims", token.ToString()));
        if (operation.HttpStatusCode != StatusCodes.Status200OK)
            return null;

        var entity = operation.Result as ClaimsEntity;

        if (entity.Recycled)
            await Claims.ExecuteAsync(TableOperation.Delete(entity));

        return entity;
    }
    public async Task<ClaimsEntity> Fetch(Guid token)
    {
        var operation = await Claims.ExecuteAsync(TableOperation.Retrieve<ClaimsEntity>("Claims", token.ToString()));
        if (operation.HttpStatusCode != StatusCodes.Status200OK)
            return null;

        var entity = operation.Result as ClaimsEntity;

        return entity;
    }
    public async Task<ClaimsEntity> Delete(Guid token)
    {
        var operation = await Claims.ExecuteAsync(TableOperation.Retrieve<ClaimsEntity>("Claims", token.ToString()));
        if (operation.HttpStatusCode != StatusCodes.Status200OK)
            return null;

        var entity = operation.Result as ClaimsEntity;

        await Claims.ExecuteAsync(TableOperation.Delete(entity));

        return entity;
    }
    public async Task<IEnumerable<ClaimsEntity>> FetchUnrecycled()
    {
        var query = new TableQuery<ClaimsEntity>()
            .Where(TableQuery.GenerateFilterConditionForBool(
                               nameof(ClaimsEntity.Recycled),
                                              QueryComparisons.Equal,
                                                             false)
                   );
        var unrecycled = new List<ClaimsEntity>();
        TableContinuationToken continuation = null;
        do
        {
            var operation = await Claims.ExecuteQuerySegmentedAsync(query, continuation);
            unrecycled.AddRange(operation.Results);
            continuation = operation.ContinuationToken;
        } while (continuation != null);
        return unrecycled;
    }

    public async Task<int> Purge()
    {
        var query = new TableQuery<ClaimsEntity>()
            .Where(TableQuery.GenerateFilterConditionForDate(
                nameof(ClaimsEntity.Expires),
                QueryComparisons.LessThan,
                DateTimeOffset.UtcNow)
        );
        var purgeBatch = new TableBatchOperation();
        var purgeCount = 0;
        void purgeClaims(ClaimsEntity entity) =>
            purgeBatch.Add(TableOperation.Delete(entity));
        TableContinuationToken continuation = null;
        do
        {
            var operation = await Claims.ExecuteQuerySegmentedAsync(query, continuation);
            operation.Results.ForEach(purgeClaims);
            if (purgeBatch.Any())
            {
                await Claims.ExecuteBatchAsync(purgeBatch);
                purgeCount += purgeBatch.Count;
                purgeBatch.Clear();
            }
            continuation = operation.ContinuationToken;
        } while (continuation != null);
        return purgeCount;
    }
}
