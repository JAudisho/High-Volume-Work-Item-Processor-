var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();  // serve index.html
app.UseStaticFiles();

// Simple dev health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "ok", timeUtc = DateTime.UtcNow }));

app.Run();
