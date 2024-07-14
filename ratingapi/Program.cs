using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Hello from Rating Api!");
});

app.MapGet("/rating/{id:int}", (int id) =>
{
    return Random.Shared.Next(1,6);
})
.WithName("GetRating")
.WithOpenApi();

app.MapPost("/rating/{id:int}", (int id, int value) =>
    {
        return Results.Ok();
    })
    .WithName("PostRating")
    .WithOpenApi();

app.Run();