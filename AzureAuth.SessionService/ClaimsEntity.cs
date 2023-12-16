using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System;

namespace AzureAuth.SessionService;

/// <summary>
/// ClaimsEntity represents a set of claims for a user or service.
/// A user session is used by a browser, and is maintained with a 
/// refresh token inside a cookie. A service session is used by 
/// automated tasks and is maintained by a custom header. 
/// </summary>
/// <remarks>
/// The main difference is that user sessions are <see cref="Recycled"/>, while
/// service tokens are not. This is because a user session is more publid
/// than services.
/// </remarks>
public class ClaimsEntity : ITableEntity
{
    /// <summary>
    /// The security / auth id of this claim set.
    /// </summary>
    public Guid Token { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Users update their security tokens every time they refresh their session,
    /// this is called recycling. Getting a recycled token from storage will
    /// delete it as well. It must be saved again with a new token if the session
    /// is to stay alive.
    /// </summary>
    /// <remarks>
    /// Services do not recycle their tokens, they are only deleted when they expire.
    /// </remarks>
    public bool Recycled { get; set; }
    /// <summary>
    /// The claims of this claim set.
    /// </summary>
    public IEnumerable<Claim> Claims { get; set; }

    /// <summary>
    /// When this claim set expires.
    /// </summary>
    public DateTimeOffset Expires { get; set; }

    /// <summary>
    /// The duration of the JWTs generated from this claim set.
    /// </summary>
    /// <remarks>
    /// This should default to 15 minutes for a user session.
    /// Service sessions are usually an hour, but long running tasks might
    /// motivate even longer durations.
    /// </remarks>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Set the claims of this claim set.
    /// </summary>
    /// <param name="claims">The new claimset</param>
    /// <returns>True, if any claims in the set changed.</returns>
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

    // ITableEntity implementation
    string ITableEntity.PartitionKey { get; set; } = "Claims";
    string ITableEntity.RowKey { get => Token.ToString(); set => Token = Guid.Parse(value); }
    DateTimeOffset ITableEntity.Timestamp { get; set; }
    string ITableEntity.ETag { get; set; }

    // The claim set is stored as a base64 encoded binary blob. Pardon the blunt serialization.
    void ITableEntity.ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
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
    IDictionary<string, EntityProperty> ITableEntity.WriteEntity(OperationContext operationContext)
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
