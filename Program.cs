using Assigment_2.Data;

var builder = WebApplication.CreateBuilder(args);


var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton<SongStore>();   
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Playlist API";
        document.Info.Description = "SE4458 Assignment 2 – Group 2: Playlist REST API (in-memory)";
        document.Servers?.Clear();   
        return Task.CompletedTask;
    });
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();


app.MapOpenApi();   
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Playlist API v1")); 

app.UseCors();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
