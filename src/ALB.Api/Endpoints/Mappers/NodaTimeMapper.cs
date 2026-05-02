using NodaTime;

namespace ALB.Api.Endpoints.Mappers;

internal static class NodaTimeMapper
{
    private static readonly DateTimeZone TimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <summary>
    /// Convert a LocalDate to a Unix timestamp in seconds
    /// </summary>
    /// <param name="localDate"></param>
    /// <returns>long</returns>
    internal static long ToUnixTimestamp(this LocalDate localDate)
        => localDate.AtStartOfDayInZone(TimeZone).ToInstant().ToUnixTimeSeconds();
    
    /// <summary>
    /// Convert a DateTime to a Unix timestamp in seconds
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns>long</returns>
    internal static long ToUnixTimestamp(this DateTime dateTime) => Instant.FromDateTimeUtc(dateTime).ToUnixTimeSeconds();
    
    /// <summary>
    /// 1. Create an Instant from the Unix timestamp in seconds
    /// </summary>
    /// <param name="unixTimestampSeconds">Timestamp in seconds</param>
    /// <returns>LocalDate</returns>
    internal static LocalDate ToLocalDate(this long unixTimestampSeconds)
    {
        Instant instant = Instant.FromUnixTimeSeconds(unixTimestampSeconds);
        LocalDate localDate = instant.InZone(TimeZone).Date;
        return localDate;
    }
}