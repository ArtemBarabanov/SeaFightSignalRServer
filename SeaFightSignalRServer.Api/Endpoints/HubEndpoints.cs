using SeaFightSignalRServer.Api.Configuration;
using SeaFightSignalRServer.Hubs.Hubs;

namespace SeaFightSignalRServer.Api.Endpoints
{
    /// <summary>
    /// Добавление хабов SignalR
    /// </summary>
    public static class HubEndpoints
    {
        /// <summary>
        /// Регистрирует хабы SignalR
        /// </summary>
        /// <param name="endpoints">Интерфейс для работы с эндпоинтами</param>
        /// <param name="configuration">Конфигурация</param>
        public static void RegisterHubEndpoints(this IEndpointRouteBuilder endpoints, AppSettings configuration)
        {
            endpoints.MapHub<GameHub>(configuration.GameHubPath);
        }
    }
}
