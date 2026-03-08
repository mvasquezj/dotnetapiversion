namespace SimpleApi.Request;

/// <summary>
/// Producto Response
/// </summary>
/// <param name="Name">Product Name</param>
/// <param name="Description">Product Description</param>
public record ProductRequest
(string Name, string Description);