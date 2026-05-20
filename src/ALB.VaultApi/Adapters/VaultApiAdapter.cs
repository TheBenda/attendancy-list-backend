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
