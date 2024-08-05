 namespace SeaFightSignalRServer.Api.Endpoints
{
    /// <summary>
    /// Добавление корневых эндпоинтов
    /// </summary>
    public static class RootEndpoints
    {
        /// <summary>
        /// Регестрирует корневые эндпоинты
        /// </summary>
        /// <param name="endpoints">Интерфейс для работы с эндпоинтами</param>
        public static void RegisterRootEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/", () =>
            {
                return "Сервер запущен!";
            })
            .WithOpenApi();
        }
    }
}
