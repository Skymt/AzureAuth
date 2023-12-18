using System.Security.Claims;

namespace AzureAuth.Core.ClaimsSerialization;
public static class ClaimsExtensions
{
    public static string SerializeToBase64(this IEnumerable<Claim> claims)
    {
        using var targetStream = new MemoryStream();
        using var serializer = new BinaryWriter(targetStream);
        foreach (var claim in claims) claim.WriteTo(serializer);
        serializer.Flush();

        return Convert.ToBase64String(targetStream.ToArray());
    }
    public static IEnumerable<Claim> DeserializeToClaims(this string base64Data)
    {
        var binaryData = Convert.FromBase64String(base64Data);
        using var sourceStream = new MemoryStream(binaryData);
        using var serializer = new BinaryReader(sourceStream);
        while (serializer.PeekChar() != -1) yield return new Claim(serializer);
    }
}
