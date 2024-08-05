using SeaFightSignalRServer.Api.Configuration;

namespace SeaFightSignalRServer.Api.Extensions
{
    /// <summary>
    /// Расширения для настройки конфигурации
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Настройка конфигурации
        /// </summary>
        /// <param name="services">Сервисы</param>
        /// <param name="configuration">Конфигурация</param>
        /// <returns>Сервисы</returns>
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(configuration);

            return services;
        }
    }
}
