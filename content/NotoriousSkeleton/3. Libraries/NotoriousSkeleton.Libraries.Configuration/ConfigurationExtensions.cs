using Microsoft.Extensions.Configuration;

namespace NotoriousSkeleton.Libraries.Configuration;

public static class ConfigurationExtensions
{
    public static T BindSection<T>(this IConfiguration configuration) where T : IConfigurationOptions, new()
    {
        T options = new T();
        configuration.GetRequiredSection(T.SECTION_KEY).Bind(options);

        return options;
    }
}
