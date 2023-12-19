using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AzureAuth.Core
{
    public class JWTManager
    {
        public static readonly string AuthCookieName = "AuthID";
        public static readonly string AuthHeaderName = $"x-{AuthCookieName}";

        readonly string issuer; readonly string audience;
        readonly TokenValidationParameters validationParameters;
        readonly SigningCredentials signingCredentials;
        readonly JwtSecurityTokenHandler tokenHandler;

        public JWTManager(IConfiguration configuration)
        {
            issuer = configuration["JWT:Issuer"]!; audience = configuration["JWT:Audience"]!;
            (signingCredentials, validationParameters) = ReadJWTConfiguration(configuration);
            tokenHandler = new();
        }

        public Task<TokenValidationResult> Validate(string token)
            => tokenHandler.ValidateTokenAsync(token, validationParameters);
        public string Generate(IEnumerable<Claim> claims, TimeSpan duration, string? audience = null)
        {
            var currentAudience = audience ?? this.audience;
            var cleanClaims = claims
                .GroupBy(c => c.Type + c.Value)
                .Select(g => g.First())
                .Where(claimIsNotCurrentAudience);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(cleanClaims),
                Expires = DateTime.UtcNow.Add(duration),
                Audience = currentAudience,
                Issuer = issuer,
                SigningCredentials = signingCredentials
            };
            var token = tokenHandler.CreateToken(tokenDescription);
            return tokenHandler.WriteToken(token);

            bool claimIsNotCurrentAudience(Claim c) => !(c.Type == "aud" && c.Value == currentAudience);
        }

        /// <summary>
        /// Reads JWT configuration from the provided configuration, and construct
        /// a signing key and sensible parameters for JWT validation.
        /// </summary>
        /// <param name="configuration">The configuration container</param>
        /// <returns>A tuple of the signing key and sensible parameters for JWT validation</returns>
        /// <remarks>If a debugger is attached to the process running this function,
        /// the audience "Developers" will automatically be allowed.</remarks>
        public static (SigningCredentials, TokenValidationParameters) ReadJWTConfiguration(IConfiguration configuration)
        {
            SymmetricSecurityKey key = new(Encoding.ASCII.GetBytes(configuration["JWT:Secret"]));
            var issuers = configuration["JWT:ValidIssuers"]?.Split(',');
            var audiences = configuration["JWT:ValidAudiences"]?.Split(',').ToHashSet();

            if (Debugger.IsAttached) audiences?.Add("Developers");

            return (new(key, SecurityAlgorithms.HmacSha256), new()
            {
                ValidateLifetime = true,
                ValidateIssuer = issuers != null,
                ValidIssuers = issuers,
                ValidateAudience = audiences != null,
                ValidAudiences = audiences,
                IssuerSigningKey = key,
            });
        }

        /// <summary>
        /// Generates a new shared secret to be put in configuration.
        /// </summary>
        /// <returns>72 random characters in a string.</returns>
        /// <remarks>This method does not update any configuration!</remarks>
        public static string GenerateNewSharedSecret()
        {
            var randomSeed = new Span<byte>(new byte[54]); // 54 bytes * 8 / 6 = 72 base64 characters
            Random.Shared.NextBytes(randomSeed);
            return Convert.ToBase64String(randomSeed);
        }
    }
}