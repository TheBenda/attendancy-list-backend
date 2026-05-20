using System.Net.Http.Json;
using System.Text.Json.Serialization;

using ALB.Domain.Adapters;
using ALB.Domain.Options;
using ALB.VaultApi.Extensions;

namespace ALB.VaultApi.Adapters;

public class VaultApiAdapter : IVaultApiAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;

    public VaultApiAdapter(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<MailgunCredentials> GetMailgunCredentials(string version = "v1", CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(VaultApiExtensions.ApiVaultClient);

        var response = await httpClient.GetAsync($"{version}/kv/data/mailgun", cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var vaultKvResponse = await response.Content.ReadFromJsonAsync<VaultKvResponse>(cancellationToken);

        if (vaultKvResponse?.Data?.Data is null)
        {
            throw new InvalidOperationException("Vault response did not contain data.");
        }

        return new MailgunCredentials
        {
            ApiKey = vaultKvResponse.Data.Data.Apikey,
            Domain = vaultKvResponse.Data.Data.SandboxDomain,
            BaseUrl = vaultKvResponse.Data.Data.BaseUrl
        };
    }
}

public class VaultKvResponse
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; }
    [JsonPropertyName("lease_id")]
    public string? LeaseId { get; set; }
    public bool Renewable { get; set; }
    [JsonPropertyName("lease_duration")]
    public int LeaseDuration { get; set; }
    [JsonPropertyName("data")]
    public VaultKvData? Data { get; set; }
    [JsonPropertyName("wrap_info")]
    public string? WrapInfo { get; set; }
    List<string>? Warnings { get; set; }
    public string? Auth { get; set; }
    [JsonPropertyName("mount_type")]
    public string MountType { get; set; }
}

public class VaultKvData
{
    public VaultKvVersion1Data Data { get; set; }
    public VaultKvMetadata Metadata { get; set; }
}

public class VaultKvVersion1Data
{
    [JsonPropertyName("apikey")]
    public string Apikey { get; set; }
    [JsonPropertyName("base-url")]
    public string BaseUrl { get; set; }
    [JsonPropertyName("sandbox-domain")]
    public string SandboxDomain { get; set; }
}

public class VaultKvMetadata
{
    public DateTime CreatedTime { get; set; }
    public object CustomMetadata { get; set; }
    public DateTime? DeletionTime { get; set; }
    public bool Destroyed { get; set; }
    public int Version { get; set; }
}

/*
public class VaultApiAdapter(VaultApiClient apiClient, ILogger<VaultApiAdapter> logger): IVaultApiAdapter
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
        try
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
        catch (Exception e)
        {
            logger.LogError(e, "Could not load mailgun credentials.");
            throw;
        }
    }
}

public class GraphApiCredentials
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string Token { get; init; }
}
*/
