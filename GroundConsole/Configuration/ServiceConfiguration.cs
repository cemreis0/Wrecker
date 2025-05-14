using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Service.Services;

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
                .AddSingleton<IAvionicsSerialCommunicationService, AvionicsSerialCommunicationService>()
                .BuildServiceProvider();
        }
    }
}
