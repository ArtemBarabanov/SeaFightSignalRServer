namespace SeaFightSignalRServer.Api.Configuration
{
    /// <summary>
    /// Конфигураци приложения
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Разрешенные хосты
        /// </summary>
        public string AllowedHosts { get; set; } = string.Empty;

        /// <summary>
        /// Путь для healthchecks
        /// </summary>
        public string HealthCheckPath { get; set; } = string.Empty;

        /// <summary>
        /// Путь для GameHub SignalR
        /// </summary>
        public string GameHubPath { get; set; } = string.Empty;
    }
}
