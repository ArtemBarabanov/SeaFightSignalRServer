using SeaFightSignalRServer.Api.Configuration;

namespace SeaFightSignalRServer.Api.Endpoints
{
    /// <summary>
    /// Добавление эндпоинтов для healthcheck
    /// </summary>
    public static class HealthCheckEndpoints
    {
        /// <summary>
        /// Регестрирует healthcheck эндпоинты
        /// </summary>
        /// <param name="endpoints">Интерфейс для работы с эндпоинтами</param>
        /// <param name="configuration">Конфигурация</param>
        public static void RegisterHealthCheckEndpoints(this IEndpointRouteBuilder endpoints, AppSettings configuration)
        {
            endpoints.MapHealthChecks(configuration.HealthCheckPath);
        }
    }
}
