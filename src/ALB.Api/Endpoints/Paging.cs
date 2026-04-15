namespace ALB.Api.Endpoints;

public record GuidCursorRequest(Guid? Cursor, int Limit);

public record GuidCursorResponse<T>(List<T> Items, GuidCursorRequest Cursor, bool HasMore);