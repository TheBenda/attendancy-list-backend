namespace ALB.Domain.Options;

public class MailgunCredentials
{
    public required string ApiKey { get; init; }
    public required string Domain { get; init; }
    public required string BaseUrl { get; init; }
}