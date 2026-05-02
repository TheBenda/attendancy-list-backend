using System.Text.Json;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace ALB.Application;

public static class DateTimeExtensions
{
    public static LocalTime ToNodaLocalTime(this DateTime dateTime)
        => LocalTime.FromTimeOnly(TimeOnly.FromDateTime(dateTime));

    public static LocalTime? ToNodaLocalTime(this DateTime? dateTime)
        => dateTime.HasValue ? LocalTime.FromTimeOnly(TimeOnly.FromDateTime(dateTime.Value)) : null;

    public static IServiceCollection AddNodaTimeJsonConverters(this IServiceCollection serviceCollection)
    {
        void ConfigureNodaTime(JsonSerializerOptions opts)
        {
            opts.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            opts.Converters.Add(NodaConverters.IntervalConverter);
            opts.Converters.Add(NodaConverters.InstantConverter);
            opts.Converters.Add(NodaConverters.LocalDateConverter);
            opts.Converters.Add(NodaConverters.LocalDateTimeConverter);
            opts.Converters.Add(NodaConverters.LocalTimeConverter);
        }

        serviceCollection.Configure<JsonSerializerOptions>(ConfigureNodaTime);
        
        serviceCollection.Configure<JsonOptions>(options =>
        {
            ConfigureNodaTime(options.SerializerOptions);
        });

        return serviceCollection;
    }
}
