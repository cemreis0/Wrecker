using Microsoft.Extensions.DependencyInjection;
using Core.Services;
using System.IO.Ports;
using Service.Services;
using Microsoft.Extensions.Logging;
using Core.Enums;
using GroundConsole.Configuration;
using System.Threading;
using Core.Models;

namespace GroundConsole
{
    class Program
    {
        static async Task Main()
        {
            var serviceProvider = ServiceConfiguration.ConfigureServices();

            var avionicsSerialCommunicationService = serviceProvider.GetRequiredService<IAvionicsSerialCommunicationService>();
            var messageSourceService = serviceProvider.GetRequiredService<IMessageSourceService>();
            var logger = serviceProvider.GetRequiredService<ILogger<AvionicsSerialCommunicationService>>();

            var groundMessageSource = messageSourceService.GetMessageSource(MessageSource.GroundStation);
            var avionicsMessageSource = messageSourceService.GetMessageSource(MessageSource.Avionics);

            var appConfig = SerialPortConfig.GetConfigurationFromUser();

            avionicsSerialCommunicationService.Connect(
                appConfig.PortName,
                appConfig.BaudRate,
                appConfig.StopBits,
                appConfig.ParityBits,
                appConfig.ReadTimeout,
                appConfig.WriteTimeout
            );

            avionicsSerialCommunicationService.AvionicsMessageReadyForProcess += (sender, e) =>
            {
                try
                {
                    var message = avionicsSerialCommunicationService.ProcessSerialQueue().Result;

                    if (message is AvionicsData avionicsData)
                    {
                        logger.LogInformation($"[{avionicsMessageSource}] [VER] {avionicsData}");
                    }
                    else if (message is AvionicsInfo avionicsInfo)
                    {
                        logger.LogInformation($"[{avionicsMessageSource}] [ENF] {avionicsInfo}");
                    }
                    else if (message is AvionicsError avionicsError)
                    {
                        logger.LogError($"[{avionicsMessageSource}] [HAT] {avionicsError}");
                    }
                    else
                    {
                        logger.LogWarning($"[{groundMessageSource}] Bilinmeyen mesaj: {message}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"[{groundMessageSource}] Veri işlenirken hata: {ex.Message}");
                }
            };

            logger.LogInformation("Çıkmak için 'q' tuşuna basın...");
            while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

            avionicsSerialCommunicationService.Disconnect();
            logger.LogInformation($"{appConfig.PortName} port bağlantısı kesildi.");
        }
    }
}
