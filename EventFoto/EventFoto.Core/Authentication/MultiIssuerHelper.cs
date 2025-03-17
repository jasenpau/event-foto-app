using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EventFoto.Core.Authentication;

public class MultiIssuerHelper
{
    private readonly string _localIssuer;
    private readonly string _localSigningKey;
    private readonly string _entraIssuer;
    private readonly string _jwksUrl;
    
    private readonly List<string> _validIssuers;
    
    private readonly HttpClient _httpClient = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(30)
    })
    {
        DefaultRequestHeaders = { { "accept", "application/json" } },
        Timeout = TimeSpan.FromSeconds(60),
    };

    public MultiIssuerHelper(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtKey))
            throw new ArgumentException("Jwt Issuer and Key are required.");
        _localIssuer = jwtIssuer;
        _localSigningKey = jwtKey;
        _validIssuers = [jwtIssuer];
        
        var entraIssuer = configuration["Entra:Issuer"];
        var entraTenantId = configuration["Entra:TenantId"];
        if (string.IsNullOrWhiteSpace(entraIssuer) || string.IsNullOrWhiteSpace(entraTenantId)) return;
        _entraIssuer = entraIssuer;
        _validIssuers.Add(entraIssuer);
        _jwksUrl = $"https://login.microsoftonline.com/{entraTenantId}/discovery/v2.0/keys";
    }
    
    public IEnumerable<string> GetIssuers() => _validIssuers;
    
    public IEnumerable<SecurityKey> IssuerSigningKeyResolver(
        string token, 
        SecurityToken securityToken, 
        string kid, 
        TokenValidationParameters validationParameter)
    {
        var issuer = securityToken.Issuer;

        if (issuer.Equals(_localIssuer))
        {
            return [new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_localSigningKey))];
        }
        
        if (issuer.Equals(_entraIssuer))
        {
            var signingKey = GetEntraSigningKey(kid);
            return signingKey is null ? [] : new[] { signingKey };
        }

        return [];
    }

    private JsonWebKey? GetEntraSigningKey(string keyId)
    {
        var response = _httpClient.GetStringAsync(_jwksUrl).Result;
        if (string.IsNullOrEmpty(response)) return null;

        var keySet = new JsonWebKeySet(response);
        var signingKey = keySet.Keys?.FirstOrDefault(k => k.KeyId == keyId);
        return signingKey;
    }
}
