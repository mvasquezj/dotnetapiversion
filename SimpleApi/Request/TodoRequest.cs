using System.ComponentModel.DataAnnotations;

namespace SimpleApi.Request;

/// <summary>
/// Todo Request example label
/// </summary>
/// <param name="Title">Todo Title label</param>
/// <param name="Description">Todo Description</param>
/// <param name="Priority">Deprecado: Todo Priority, No usar en v2</param>
public record TodoRequest(
    string Title, 
    string Description, 
    [property:Obsolete]
    int Priority);