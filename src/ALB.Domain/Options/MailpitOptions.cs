namespace ALB.Domain.Options;

public class MailpitOptions
{
    public const string SectionName = "Mailpit";

    public required string Host { get; set; }
    public required int Port { get; set; }
}