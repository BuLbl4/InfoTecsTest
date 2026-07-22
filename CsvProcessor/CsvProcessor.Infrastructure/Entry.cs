using CsvProcessor.Application.Interfaces;
using CsvProcessor.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CsvProcessor.Infrastructure;

public static class Entry
{
    public static void AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddScoped<ICsvParser, CsvParser>();
    }
    
}