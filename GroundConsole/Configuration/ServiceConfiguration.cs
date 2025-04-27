using Microsoft.Extensions.DependencyInjection;
using Service.Services;
using Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace GroundConsole.Configuration
{
    public static class ServiceConfiguration
    {
        public static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging(builder => builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.TimestampFormat = "[dd-MM-yyyy HH:mm:ss] ";
                    options.ColorBehavior = LoggerColorBehavior.Enabled;
                    options.SingleLine = true;
                }))
                .AddSingleton<IMessageSourceService, MessageSourceService>()
                .AddSingleton<ILogger, Logger<AvionicsSerialCommunicationService>>()
                .AddSingleton<IAvionicsSerialCommunicationService, AvionicsSerialCommunicationService>()
                .BuildServiceProvider();
        }
    }
}
