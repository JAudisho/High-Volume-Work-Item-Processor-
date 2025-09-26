using AtlasQueue.Api.Middleware;
using AtlasQueue.Api.Processing;
using AtlasQueue.Infrastructure.Extensions;
using AtlasQueue.Infrastructure.Persistence;
using AtlasQueue.Infrastructure.Persistence.Seed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// CORS so Web (5002) can call API (5000)
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5002").AllowAnyHeader().AllowAnyMethod()));

// Infrastructure (EF Core / DI)
var cs = builder.Configuration.GetConnectionString("SqlServer")!;
builder.Services.AddInfrastructure(cs);

// Background queue + worker
builder.Services.AddSingleton<IWorkItemChannel>(_ => new WorkItemChannel(capacity: 20_000));
builder.Services.AddHostedService<QueueWorker>();

var app = builder.Build();

// Seed DB on boot (creates DB + sample items if empty)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();