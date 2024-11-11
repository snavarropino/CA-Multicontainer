using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

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
    await context.Response.WriteAsync("Hit the /albums endpoint to retrieve a list of albums!");
});

app.MapGet("/albums", async ([FromServices]IHttpClientFactory factory,[FromServices] IConfiguration configuration ) =>
    {
        return await Album.GetAllAsync(configuration,factory);
    })
.WithName("GetAlbums");

app.Run();



record Album(int Id, string Title, string Artist, double Price, string Image_url, int Rating)
{
     public static async Task<List<Album>> GetAllAsync(IConfiguration configuration, IHttpClientFactory httpClientFactory)
     {
         var albums = new List<Album>{
            new Album(1, "You, Me and an App Id", "Daprize", 10.99, "https://aka.ms/albums-daprlogo", await GetRating(configuration, httpClientFactory, 1)),
            new Album(2, "Seven Revision Army", "The Blue-Green Stripes", 13.99, "https://aka.ms/albums-containerappslogo",await GetRating(configuration, httpClientFactory, 2)),
            new Album(3, "Scale It Up", "KEDA Club", 13.99, "https://aka.ms/albums-kedalogo",await GetRating(configuration, httpClientFactory, 3)),
            new Album(4, "Lost in Translation", "MegaDNS", 12.99,"https://aka.ms/albums-envoylogo",await GetRating(configuration, httpClientFactory, 4)),
            new Album(5, "Lock Down Your Love", "V is for VNET", 12.99, "https://aka.ms/albums-vnetlogo",await GetRating(configuration, httpClientFactory, 5)),
            new Album(6, "Sweet Container O' Mine", "Guns N Probeses", 14.99, "https://aka.ms/albums-containerappslogo",await GetRating(configuration, httpClientFactory, 6))
         };

        return albums; 
     }

     static async Task<int> GetRating(IConfiguration configuration, IHttpClientFactory httpClientFactory, int id)
     {
         var ratingApiBaseUrl=configuration["RatingApiBaseUrl"];
         var httpClient = httpClientFactory.CreateClient();
         var i= await httpClient.GetFromJsonAsync<int>($"{ratingApiBaseUrl}/rating/{id}");
         return i;
     }
}