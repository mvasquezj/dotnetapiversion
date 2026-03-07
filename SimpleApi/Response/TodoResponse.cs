namespace SimpleApi.Response;

public record TodoResponse
( Guid Id, string Title, string Description, int Priority);