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
    private static Dictionary<string, Dictionary<string, dynamic>?> _inMemoryDoc = new ();
    
    
    /// <summary>
    /// Init method
    /// </summary>
    private static Dictionary<string, dynamic>? GetVersionDoc(OpenApiOptions options)
    {
        if (_inMemoryDoc.TryGetValue(options.DocumentName, out var versionConfig)) return versionConfig;
        
        var yamlContent = File.ReadAllText($@"Docs/api-docs-{options.DocumentName}.yaml");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
            
        versionConfig = deserializer.Deserialize<Dictionary<string, dynamic>>(yamlContent);

        _inMemoryDoc.Add(options.DocumentName, versionConfig);

        return versionConfig;
    }

    /// <summary>
    /// YAML transformation work with file in path "Docs/api-docs-{version}.yaml"
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static OpenApiOptions AddSimpleYamlTransformation(this OpenApiOptions options)
    {
        var versionDoc =  GetVersionDoc(options);
        
        options.AddDocumentTransformer((document, _, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
                
            document.Info.Title = versionDoc!["info"]["title"];
            document.Info.Description = versionDoc["info"]["description"];

            var infoConfig = (IDictionary<object, object>) versionDoc["info"];
            
            if(versionDoc.ContainsKey("contact"))
                document.Info.Contact = new OpenApiContact()
                {
                    Email = versionDoc["info"]["contact"]["email"],
                    Name =  versionDoc["info"]["contact"]["name"]
                };
            
            if(infoConfig.ContainsKey("version"))
                document.Info.Version = versionDoc["info"]["version"];
            
            if(infoConfig.ContainsKey("summary"))
                document.Info.Summary = versionDoc["info"]["summary"];
            
            if(infoConfig.ContainsKey("termsOfService"))
                document.Info.TermsOfService = versionDoc["info"]["termsOfService"];
            
            return Task.CompletedTask;
        });

        options.AddOperationTransformer(async (operation, context, cancellationToken) =>
        {
            var operations = (IDictionary<object, object>) versionDoc!["operations"];
            if (operation.OperationId == null || !operations.ContainsKey(operation.OperationId!)) return;
            
            var operationConfig = (IDictionary<object, object>) versionDoc["operations"][operation.OperationId!];
            
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