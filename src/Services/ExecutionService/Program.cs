using System.Reflection;
using Carter;
using ExecutionService.Infrastrcture;
using IdGen.DependencyInjection;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdGen(2);

builder.Services.AddSharedInfrastructure([Assembly.GetExecutingAssembly()]);

builder.Services.AddExecutionInfrastructure(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
   {
       options.DocumentPath = "openapi/v1.json";
   });
}
app.UseExceptionHandler();
app.MapCarter();

app.Run();
