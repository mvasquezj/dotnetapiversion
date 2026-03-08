namespace SimpleApi.Response;

/// <summary>
/// Todo Response
/// </summary>
/// <param name="Id">Todo Id</param>
/// <param name="Title">Todo Title</param>
/// <param name="Description">Todo Description</param>
/// <param name="Priority">Todo Priority</param>
public record TodoResponse
( Guid Id, string Title, string Description, int Priority);