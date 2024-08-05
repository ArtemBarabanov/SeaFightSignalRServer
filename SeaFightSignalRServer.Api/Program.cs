using SeaFightSignalRServer.Api.Configuration;
using SeaFightSignalRServer.Api.Endpoints;
using SeaFightSignalRServer.Api.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var baseConfiguration = builder.Configuration.Get<AppSettings>() ?? new AppSettings();

    builder.Services.AddConfiguration(builder.Configuration);
    builder.AddSerilog();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = true;
        options.KeepAliveInterval = TimeSpan.FromSeconds(5);
        options.HandshakeTimeout = TimeSpan.FromSeconds(5);
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI();
    }

    app.RegisterRootEndpoints();
    app.RegisterHealthCheckEndpoints(baseConfiguration);
    app.RegisterHubEndpoints(baseConfiguration);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal($"Ошибка при запуске приложения! {ex.Message}");
}
finally
{
    Log.CloseAndFlush();
}
