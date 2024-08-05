using Serilog;

namespace SeaFightSignalRServer.Api.Extensions
{
    /// <summary>
    /// Расширения для Serilog
    /// </summary>
    public static class SerilogExtensions
    {
        /// <summary>
        /// Подключение Serilog
        /// </summary>
        /// <param name="builder"><inheritdoc cref="WebApplicationBuilder"/></param>
        public static void AddSerilog(this WebApplicationBuilder builder)
        {
            builder.Host
                .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
        }
    }
}
