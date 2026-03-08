namespace SimpleApi.Response;

/// <summary>
/// Producto Response
/// </summary>
/// <param name="Id">Product ID</param>
/// <param name="Name">Product Name</param>
/// <param name="Description">Product Description</param>
public record ProductResponse
(Guid Id, 
    string Name, 
    string Description);