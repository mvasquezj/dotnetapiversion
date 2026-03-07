namespace SimpleApi.Response;


/// <summary>
/// Error Business Object
/// </summary>
/// <param name="Code">Error Code</param>
/// <param name="Message">Error Message</param>
public record BusinessError(
    string Code, 
    string Message);