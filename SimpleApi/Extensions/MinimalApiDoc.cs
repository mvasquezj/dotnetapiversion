using FluentValidation.Results;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SimpleApi.Response;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SimpleApi.Extensions;

/// <summary>
/// Extension metod for read docs from settings based on convention
/// </summary>
public static class MinimalApiDoc   
{
    /// <summary>
    /// Config as dictionary
    /// </summary>
    private static Dictionary<string, dynamic>? _inMemoryDoc;
    
    
    /// <summary>
    /// Init method
    /// </summary>
    private static void InitConfig()
    {
        var yamlContent = File.ReadAllText($@"Docs/api-docs.yaml");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _inMemoryDoc ??= deserializer.Deserialize<Dictionary<string, dynamic>>(yamlContent);
    }

    /// <summary>
    /// YAML transformation work with file in path "Docs/api-docs-{version}.yaml"
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static OpenApiOptions AddSimpleYamlTransformation(this OpenApiOptions options)
    {
        InitConfig();
        
        options.AddDocumentTransformer((document, _, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
                
            document.Info.Title = _inMemoryDoc!["info"]["title"];
            document.Info.Description = _inMemoryDoc["info"]["description"];

            var infoConfig = (IDictionary<object, object>) _inMemoryDoc["info"];
            
            if(infoConfig.ContainsKey("contact"))
                document.Info.Contact = new OpenApiContact()
                {
                    Email = _inMemoryDoc["info"]["contact"]["email"],
                    Name =  _inMemoryDoc["info"]["contact"]["name"]
                };
            
            if(infoConfig.ContainsKey("version"))
                document.Info.Version = _inMemoryDoc["info"]["version"];
            
            if(infoConfig.ContainsKey("summary"))
                document.Info.Summary = _inMemoryDoc["info"]["summary"];
            
            if(infoConfig.ContainsKey("termsOfService"))
                document.Info.TermsOfService = _inMemoryDoc["info"]["termsOfService"];
            
            return Task.CompletedTask;
        });

        options.AddOperationTransformer(async (operation, context, cancellationToken) =>
        {
            var operations = (IDictionary<object, object>)_inMemoryDoc!["operations"];
            if (operation.OperationId == null || !operations.ContainsKey(operation.OperationId!)) return;
            
            var operationConfig = (IDictionary<object, object>) _inMemoryDoc["operations"][operation.OperationId!];
            
            operation.Summary = operationConfig["summary"].ToString()!;
            
            if (operationConfig.TryGetValue("description", out var value1))
            {
                operation.Description = value1.ToString()!;
            }
            
            if (operationConfig.TryGetValue("internal_server_error_500", out var value))
            {
                await TransformResponse(operation, context, "500",
                    value.ToString()!, cancellationToken);
            }
            
            if (operationConfig.TryGetValue("conflict_409", out var value2))
            {
                await TransformResponse(operation, context, "409",
                    value2.ToString()!, cancellationToken, typeof(BusinessError));
            }
            
            if (operationConfig.TryGetValue("not_found_404", out var value3))
            {
                await TransformResponse(operation, context, "404",
                    value3.ToString()!, cancellationToken);
            }
            
            if (operationConfig.TryGetValue("bad_request_400", out var value4))
            {
                await TransformResponse(operation, context, "400",
                    value4.ToString()!, cancellationToken, typeof(ValidationResult));
            }
        });
        
        return options;
    }

    private static async Task TransformResponse(OpenApiOperation operation, OpenApiOperationTransformerContext context,  string statusCode, 
        string description, CancellationToken cancellationToken, Type? type = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!operation.Responses!.TryGetValue(statusCode, out var response))
        {
            response = new OpenApiResponse();
            operation.Responses.Add(statusCode, response);
        }
        
        response.Description = description;
        
        if (type != null && response.Content != null && !response.Content.ContainsKey("application/json"))
        {
            var schema = await context.GetOrCreateSchemaAsync(type, cancellationToken: cancellationToken);
            response.Content.Add("application/json", new OpenApiMediaType
            {
                Schema = schema
            });
        }
        
        operation.Responses[statusCode] = response;
    }
    
    /// <summary>
    /// Extension metod for read docs from settings based on convention
    /// </summary>
    /// <param name="builder">Route builder</param>
    /// <returns></returns>
    public static RouteHandlerBuilder WithBaseLineDoc(this RouteHandlerBuilder builder)
    {
        builder.Produces(StatusCodes.Status423Locked);
        builder.Produces(StatusCodes.Status500InternalServerError);
        return builder;
    }
}