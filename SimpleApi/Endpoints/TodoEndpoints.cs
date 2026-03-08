using Asp.Versioning;
using FluentValidation.Results;
using SimpleApi.Extensions;
using SimpleApi.Request;
using SimpleApi.Response;

namespace SimpleApi.Endpoints;

/// <summary>
/// Todo Endpoints
/// </summary>
public static class TodoEndpoints
{
    /// <summary>
    /// Extension methos for add endpoints
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseTodoEndpoints(this WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();
        
        var versionedGroup = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);
        
        versionedGroup.MapGet("/todos", () =>
            {
                return Results.Ok(Array.Empty<TodoResponse>());
            })
            .WithName("getTodos")
            .WithTags("Todo")
            .MapToApiVersion(1, 0)
            .MapToApiVersion(2, 0)
            .Produces<IEnumerable<TodoResponse>>()
            .WithBaseLineDoc();
        
        versionedGroup.MapGet("/todos/{id}", (Guid id) =>
            {
                return Results.Ok(default(TodoResponse));
            })
            .WithName("getTodoById")
            .WithTags("Todo")
            .MapToApiVersion(1, 0)
            .Produces<TodoResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .WithBaseLineDoc();
        
        versionedGroup.MapGet("/todos/{code}", (string code) =>
            {
                return Results.Ok(default(TodoResponse));
            })
            .WithName("getTodoByCode")
            .WithTags("Todo")
            .MapToApiVersion(2, 0)
            .Produces<TodoResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .WithBaseLineDoc();
            
        
        versionedGroup.MapPost("/todos", (TodoRequest todoRequest) =>
            {
                return Results.Ok(todoRequest);
            })
            .WithName("addTodo")
            .WithTags("Todo")
            .MapToApiVersion(1, 0)
            .MapToApiVersion(2, 0)
            .Produces<TodoResponse>()
            .Produces<ValidationResult>(StatusCodes.Status400BadRequest)
            .Produces<BusinessError>(StatusCodes.Status409Conflict)
            .WithBaseLineDoc();
        return app;
    }
}