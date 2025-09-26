using AtlasQueue.Domain.Abstractions;
using AtlasQueue.Infrastructure.Persistence;
using AtlasQueue.Infrastructure.Repositories;
using AtlasQueue.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasQueue.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    // This is what Program.cs calls:
    // builder.Services.AddInfrastructure(cs);
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Use SQL Server (matches your appsettings.json)
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("AtlasQueueDb"));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IProcessor, WorkItemProcessor>();

        return services;
    }
}
