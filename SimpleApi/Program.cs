using Asp.Versioning;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using SimpleApi.Endpoints;
using SimpleApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var delegateYamlTransformation = void (OpenApiOptions options) => options.AddSimpleYamlTransformation();

builder.Services.AddOpenApi("v1", delegateYamlTransformation);
builder.Services.AddOpenApi("v2", delegateYamlTransformation);
builder.Services.AddControllers();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
    options.DefaultApiVersion = new ApiVersion(2, 0);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v2.json", "v2");
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
    app.MapScalarApiReference(options =>
    {
        options
            .AddDocument("v1", "API Version 1.0", "/openapi/v1.json")
            .AddDocument("v2", "API Version 2.0", "/openapi/v2.json", isDefault:true); 
    });
}

app.UseHttpsRedirection();
app.UseTodoEndpoints();

app.Run();
