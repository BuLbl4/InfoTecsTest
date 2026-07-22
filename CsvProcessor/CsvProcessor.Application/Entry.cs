using Microsoft.Extensions.DependencyInjection;

namespace CsvProcessor.Application;

public static class Entry
{
    public static void AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Entry).Assembly));
    }
}