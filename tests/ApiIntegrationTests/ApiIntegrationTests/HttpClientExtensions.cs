using System.Net.Http.Json;
using System.Text.Json;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace ApiIntegrationTests;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    
    static HttpClientExtensions()
    {
        Options.Converters.Add(NodaConverters.IntervalConverter);
        Options.Converters.Add(NodaConverters.InstantConverter);
        Options.Converters.Add(NodaConverters.LocalDateConverter);
        Options.Converters.Add(NodaConverters.LocalDateTimeConverter);
        Options.Converters.Add(NodaConverters.LocalTimeConverter);
    }

    public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value)
    {
        return HttpClientJsonExtensions.PostAsJsonAsync(client, requestUri, value, Options);
    }
    
    public static Task<HttpResponseMessage> PutAsJsonAsync<TValue>(this HttpClient client, string? requestUri, TValue value)
    {
        return HttpClientJsonExtensions.PutAsJsonAsync(client, requestUri, value, Options);
    }
    
    public static Task<TValue?> ReadFromJsonAsync<TValue>(this HttpContent content, CancellationToken cancellationToken = default)
    {
        return HttpContentJsonExtensions.ReadFromJsonAsync<TValue>(content, Options, cancellationToken);
    }
    
    public static Task<TValue?> GetFromJsonAsync<TValue>(this HttpClient client, string? requestUri, CancellationToken cancellationToken = default)
    {
        return HttpClientJsonExtensions.GetFromJsonAsync<TValue>(client, requestUri, Options, cancellationToken);
    }
}
