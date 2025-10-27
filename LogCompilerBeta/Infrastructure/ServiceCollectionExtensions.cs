using LogCompilerBeta.Interfaces.ContentReader;
using LogCompilerBeta.Interfaces.Factory;
using LogCompilerBeta.Services.ContentReaders;
using LogCompilerBeta.Services.Factory;

namespace LogCompilerBeta.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContentReaders(this IServiceCollection services)
        {
            services.AddScoped<SmallFileContentReader>();
            services.AddScoped<MediumFileContentReader>();
            services.AddScoped<LargeFileContentReader>();
            services.AddScoped<VeryLargeFileContentReader>();

            services.AddScoped<IContentReader, SmallFileContentReader>();
            services.AddScoped<IContentReader, MediumFileContentReader>();
            services.AddScoped<IContentReader, LargeFileContentReader>();
            services.AddScoped<IContentReader, VeryLargeFileContentReader>();

            services.AddScoped<IContentReaderFactory, ContentReaderFactory>();

            return services;
        }
    }
}
