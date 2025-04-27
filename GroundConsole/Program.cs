using Microsoft.Extensions.DependencyInjection;
using Core.Services;
using System.IO.Ports;
using Service.Services;
using Microsoft.Extensions.Logging;
using Core.Enums;
using GroundConsole.Configuration;
using System.Threading;

namespace GroundConsole
{
    class Program
    {
        static async Task Main()
        {
            var serviceProvider = ServiceConfiguration.ConfigureServices();

            var appConfig = SerialPortConfig.GetConfigurationFromUser();

            var avionicsSerialCommunicationService = serviceProvider.GetRequiredService<IAvionicsSerialCommunicationService>();
            var logger = serviceProvider.GetRequiredService<ILogger<AvionicsSerialCommunicationService>>();

            avionicsSerialCommunicationService.Connect(
                appConfig.PortName,
                appConfig.BaudRate,
                appConfig.StopBits,
                appConfig.ParityBits,
                appConfig.ReadTimeout,
                appConfig.WriteTimeout
            );

            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    await avionicsSerialCommunicationService.ProcessSerialQueue();
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Servis durduruldu.");
                }
            }

            logger.LogInformation("Çıkmak için 'q' tuşuna basın...");
            while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

            avionicsSerialCommunicationService.Disconnect();
            logger.LogInformation($"{appConfig.PortName} port bağlantısı kesildi.");
        }
    }
}
