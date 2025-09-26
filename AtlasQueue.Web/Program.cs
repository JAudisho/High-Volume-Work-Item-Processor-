var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();  // serve index.html
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "ok", timeUtc = DateTime.UtcNow }));

app.Run();
