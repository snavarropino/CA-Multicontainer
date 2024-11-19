var builder = WebApplication.CreateBuilder(args);

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

var ratings = new Dictionary<int, int>();

app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Hello from Rating Api!");
});

app.MapGet("/rating/{id:int}", (int id) =>
{
    if (!ratings.ContainsKey(id))
    {
        ratings[id] = Random.Shared.Next(1, 6);
    }
    return ratings[id];
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
