using VaultApi.Client;

namespace ALB.VaultApi.Adapters;

public class VaultApiAdapter(VaultApiClient apiClient): IVaultApiAdapter
{
    private readonly string _emailKey = "graphEmail";
    private readonly string _passwordKey = "graphPassword";
    private readonly string _tokenKey = "graphToken";
    
    private readonly string _apikeyKey = "apikey";
    private readonly string _baseUrlKey = "base-url";
    private readonly string _sandboxDomainKey = "sandbox-domain";
    
    public async Task<GraphApiCredentials> GetToken()
    {
        var credentialsResponse = await apiClient.Kv.Data["graph_credentials"].GetAsync();
        
        var outerData = credentialsResponse?.Data;
        if (outerData is null)
            throw new InvalidOperationException("Vault response did not contain data.");

        var data = outerData.GetFieldDeserializers();
        
        return new GraphApiCredentials
        {
            Email = data[_emailKey].ToString() ?? "",
            Password = data[_passwordKey].ToString() ?? "",
            Token = data[_tokenKey].ToString() ?? ""
        };
    }
    
    public async Task<MailgunCredentials> GetMailgunCredentials()
    {
        var credentialsResponse = await apiClient.Kv.Data["mailgun"].GetAsync();
        
        var outerData = credentialsResponse?.Data;
        if (outerData is null)
            throw new InvalidOperationException("Vault response did not contain data.");
        
        var data = outerData.GetFieldDeserializers();
        
        return new MailgunCredentials
        {
            ApiKey = data[_apikeyKey].ToString() ?? "",
            Domain = data[_sandboxDomainKey].ToString() ?? "",
            BaseUrl = data[_baseUrlKey].ToString() ?? ""
        };
    }
}

public class GraphApiCredentials
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string Token { get; init; }
}

public class MailgunCredentials
{
    public required string ApiKey { get; init; }
    public required string Domain { get; init; }
    public required string BaseUrl { get; init; }
    
}