using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace AzureAuth.SessionService;

public class ClaimsRepository
{
    readonly CloudTable Claims;
    public ClaimsRepository(CloudStorageAccount account)
    {
        var client = account.CreateCloudTableClient();

        Claims = client.GetTableReference("Claims");
        Claims.CreateIfNotExistsAsync().Wait();
    }
    public ClaimsRepository(IConfiguration configuration)
        : this(CloudStorageAccount.Parse(configuration.GetConnectionString("AzureWebJobsStorage")))
    { }
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
public class ClaimsEntity : ITableEntity
{
    public Guid Token { get; set; } = Guid.NewGuid();
    public bool Recycled { get; set; }
    public Claim[] Claims { get; set; }
    public DateTimeOffset Expires { get; set; }
    public TimeSpan Duration { get; set; }

    public string PartitionKey { get; set; } = "Claims";
    public string RowKey { get => Token.ToString(); set => Token = Guid.Parse(value); }
    public DateTimeOffset Timestamp { get; set; }
    public string ETag { get; set; }

    public bool SetClaims(IEnumerable<Claim> claims)
    {
        if(Claims == null)
        {
            Claims = claims.ToArray();
            return true;
        }

        bool dirty = false;
        List<Claim> result = new();
        foreach (var claim in claims)
        {
            var oldClaim = Claims.FirstOrDefault(c => c.Type ==  claim.Type);
            if (oldClaim != null && oldClaim.Value != claim.Value) { dirty = true; } 
            result.Add(claim);
        }
        Claims = result.ToArray();
        return dirty;
    }

    public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
    {
        Expires = properties[nameof(Expires)].DateTimeOffsetValue.Value;
        Duration = TimeSpan.FromMilliseconds(properties[nameof(Duration)].DoubleValue.Value);
        Recycled = true;
        if (Expires > DateTimeOffset.Now)
        {
            Recycled = properties[nameof(Recycled)].BooleanValue.Value;
            var base64Data = properties["Claims"].StringValue;
            var binaryData = Convert.FromBase64String(base64Data);
            using var sourceStream = new MemoryStream(binaryData);
            using var serializer = new BinaryReader(sourceStream);
            List<Claim> claims = new();
            while (serializer.PeekChar() != -1) claims.Add(new Claim(serializer));
            Claims = claims.ToArray();
        }
    }
    public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
    {
        using var targetStream = new MemoryStream();
        using var serializer = new BinaryWriter(targetStream);
        foreach (var claim in Claims) claim.WriteTo(serializer);
        serializer.Flush();

        var binaryData = targetStream.ToArray();
        var base64Data = Convert.ToBase64String(binaryData);

        return new Dictionary<string, EntityProperty>
        {
            [nameof(Expires)] = EntityProperty.GeneratePropertyForDateTimeOffset(Expires),
            [nameof(Duration)] = EntityProperty.GeneratePropertyForDouble(Duration.TotalMilliseconds),
            [nameof(Recycled)] = EntityProperty.GeneratePropertyForBool(Recycled),
            [nameof(Claims)] = EntityProperty.GeneratePropertyForString(base64Data)
        };
    }
}
