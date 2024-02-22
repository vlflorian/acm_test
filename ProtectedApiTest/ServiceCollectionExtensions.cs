namespace AcmTest;

public static class ServiceCollectionExtensions
{
    public static T RegisterOptions<T>(this IServiceCollection services, IConfiguration configuration, string? configKey = null)
        where T : class
    {
        var configurationKey = configKey ?? typeof(T).Name;
        services
            .AddOptions<T>()
            .BindConfiguration(configurationKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = configuration.GetRequiredSection(configurationKey).Get<T>();

        return options;
    }
}