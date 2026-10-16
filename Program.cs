using Assigment_2.Data;

var builder = WebApplication.CreateBuilder(args);

// Render / Vercel portu PORT ortam değişkeniyle verir
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton<SongStore>();   // in-memory veritabanı
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Playlist API";
        document.Info.Description = "SE4458 Assignment 2 – Group 2: Playlist REST API (in-memory)";
        return Task.CompletedTask;
    });
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Swagger production'da da açık (ödev şartı)
app.MapOpenApi();   // JSON: /openapi/v1.json
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Playlist API v1"));  // UI: /swagger

app.UseCors();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();