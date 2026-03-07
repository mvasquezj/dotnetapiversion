using System.Reflection;
using SimpleApi.Request;
using SimpleApi.Response;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options => {
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

app.UseHttpsRedirection();


app.MapGet("/api/todos", () =>
    {

    })
    .Produces<IEnumerable<TodoResponse>>()
    .Produces<BusinessError>(StatusCodes.Status409Conflict)
    .Produces(StatusCodes.Status500InternalServerError)
    .WithSummary("Get Todos")
    .WithMetadata(new SwaggerResponseAttribute(409, "Conflicto de negocio. Posibles códigos:\n- `USER_EXISTS`: El correo ya está registrado.\n- `ROLE_REQUIRED`: El usuario debe tener un rol asignado.", typeof(BusinessError)))
    .WithDescription( """
                      Errores de validación posibles:  
                      - **USER_EXISTS**: El usuario ya está en la base de datos.  
                      - **INVALID_EMAIL**: El formato del correo no es válido.

                      *Nota: Verifique los campos antes de reintentar.*
                      """);

app.MapPost("/api/todos", (TodoRequest todoRequest) =>
    {
        
    })
    .Produces<TodoResponse>()
    .WithSummary("Add Todos");

app.Run();
