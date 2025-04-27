using Core.Enums;
using Core.Models;
using Core.Services;
using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace Service.Services
{
    public class AvionicsSerialCommunicationService : IAvionicsSerialCommunicationService
    {
        private readonly ILogger _logger;
        private readonly IMessageSourceService _messageSourceService;

        private readonly SerialPort _serialPort;

        private readonly Queue<byte> _serialDataQueue;
        private readonly object _serialDataLock;
        private bool _isProcessingSerialDataQueue;

        private const int serialQueueProcessSizeMax = 50;
        private const int rawDataSizeMin = 100;

        public event EventHandler<AvionicsBase>? AvionicsMessageReceived;

        public AvionicsSerialCommunicationService(ILogger<AvionicsSerialCommunicationService> logger, IMessageSourceService messageSourceService)
        {
            _logger = logger;
            _messageSourceService = messageSourceService;

            _serialPort = new SerialPort();

            _serialDataQueue = new();
            _serialDataLock = new();
            _isProcessingSerialDataQueue = false;

            _serialPort.DataReceived += OnAvionicsMessageReceived;
        }

        public bool Connect(string portName, int baudRate, StopBits stopBits, Parity parityBits, int readTimeout, int writeTimeout)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _logger.LogWarning($"[{_messageSourceService.GetMessageSource(MessageSource.GroundStation)}] {_serialPort.PortName} halihazırda açık. Tekrar bağlanmak için port kapatılıyor.");
                    _serialPort.Close();
                }

                _serialPort.PortName = portName;
                _serialPort.BaudRate = baudRate;
                _serialPort.StopBits = stopBits;
                _serialPort.Parity = parityBits;
                _serialPort.ReadTimeout = readTimeout;
                _serialPort.WriteTimeout = writeTimeout;

                _serialPort.Open();

                _logger.LogInformation($"[{_messageSourceService.GetMessageSource(MessageSource.GroundStation)}] {portName} portuna bağlanıldı.");
                _logger.LogInformation($"[{_messageSourceService.GetMessageSource(MessageSource.GroundStation)}] Bağlantı bilgileri: BaudRate: {baudRate}, StopBits: {stopBits}, Parity: {parityBits}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{_messageSourceService.GetMessageSource(MessageSource.GroundStation)}] {portName} portuna bağlanılamadı. {ex.Message}");
                return false;
            }
        }
        public bool Disconnect()
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _logger.LogInformation($"[{_messageSourceService.GetMessageSource(MessageSource.GroundStation)}] Port bağlantısı kapatıldı.");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{_messageSourceService.GetMessageSource(MessageSource.GroundStation)}]Port bağlantısı kapatılamadı. {ex.Message}");
                return false;
            }
        }

        private void OnAvionicsMessageReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (_serialPort.BytesToRead > 0)
            {
                byte b = (byte)_serialPort.ReadByte();
                lock (_serialDataLock)
                {
                    _serialDataQueue.Enqueue(b);
                }
            }

            if (!_isProcessingSerialDataQueue && _serialDataQueue.Count >= rawDataSizeMin)
            {
                _isProcessingSerialDataQueue = true;
                Task.Run(ProcessSerialQueue);
            }
        }

        public async Task ProcessSerialQueue()
        {
            try
            {
                List<byte> rawDataList = [];

                while (true)
                {
                    lock (_serialDataLock)
                    {
                        while (_serialDataQueue.Count > 0 && rawDataList.Count < serialQueueProcessSizeMax)
                        {
                            rawDataList.Add(_serialDataQueue.Dequeue());
                        }
                    }

                    if (rawDataList.Count == 0)
                    {
                        await Task.Delay(50);
                        continue;
                    }

                    AvionicsBase avionicsMessage = ConvertRawAvionicsMessage(rawDataList);

                    if (avionicsMessage != null && avionicsMessage.Header != '\0')
                    {
                        AvionicsMessageReceived?.Invoke(this, avionicsMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{_messageSourceService.GetMessageSource(MessageSource.GroundStation)}] Kuyruğu işlerken hata meydana geldi. {ex.Message}");
            }
            finally
            {
                _isProcessingSerialDataQueue = false;
            }
        }

        private AvionicsBase ConvertRawAvionicsMessage(List<byte> rawDataList)
        {
            AvionicsBase avionicsMessage = new();

            while (rawDataList.Count > 0)
            {
                byte header = rawDataList[0];
                byte footer = 10;
                int footerIndex = rawDataList.IndexOf(10);

                if (header == (byte)'V' && rawDataList.Count >= 30)
                {
                    if (footerIndex != -1 && footerIndex < 30)
                    {
                        AvionicsData avionicsData = ExtractAvionicsData(rawDataList, (char)header, (char)footer);

                        _logger.LogInformation($"[{_messageSourceService.GetMessageSource(MessageSource.Avionics)}] [V] {avionicsData}");

                        rawDataList.RemoveRange(0, 30);

                        avionicsMessage = avionicsData;
                    }
                    else if (footerIndex == -1)
                    {
                        break;
                    }
                    else
                    {
                        _logger.LogWarning($"[{_messageSourceService.GetMessageSource(MessageSource.Avionics)}] Veri boyutu yetersiz.");
                        rawDataList.RemoveAt(0);
                    }
                }
                else if (header == (byte)'E')
                {
                    if (footerIndex != -1)
                    {
                        AvionicsInfo avionicsInfo = ExtractAvionicsInfo(rawDataList, (char)header, (char)footer, footerIndex);

                        _logger.LogInformation($"[{_messageSourceService.GetMessageSource(MessageSource.Avionics)}] [E] {avionicsInfo}");

                        rawDataList.RemoveRange(0, footerIndex + 1);

                        avionicsMessage = avionicsInfo;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (header == (byte)'H')
                {
                    if (footerIndex != -1)
                    {
                        AvionicsError avionicsError = ExtractAvionicsError(rawDataList, (char)header, (char)footer, footerIndex);

                        _logger.LogInformation($"[{_messageSourceService.GetMessageSource(MessageSource.Avionics)}] [E] {avionicsError}");

                        rawDataList.RemoveRange(0, footerIndex + 1);

                        avionicsMessage = avionicsError;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    rawDataList.RemoveAt(0);
                }
            }

            return avionicsMessage;
        }

        private AvionicsData ExtractAvionicsData(List<byte> rawDataList, char header, char footer)
        {
            var avionicsData = new AvionicsData
            {
                Header = (char)header,
                Footer = (char)footer,
                ReceivedAt = DateTime.Now
            };

            var floats = new float[7];

            for (int i = 0; i < 7; i++)
            {
                floats[i] = BitConverter.ToSingle(rawDataList.Skip(1 + i * 4).Take(4).ToArray(), 0);
            }

            avionicsData.Roll = floats[0];
            avionicsData.Pitch = floats[1];
            avionicsData.Yaw = floats[2];
            avionicsData.Altitude = floats[3];
            avionicsData.Speed = floats[4];
            avionicsData.Latitude = floats[5];
            avionicsData.Longitude = floats[6];

            return avionicsData;
        }

        private AvionicsInfo ExtractAvionicsInfo(List<byte> rawDataList, char header, char footer, int footerIndex)
        {
            var message = System.Text.Encoding.ASCII.GetString(rawDataList.Skip(1).Take(footerIndex - 1).ToArray());

            AvionicsInfo avionicsInfo = new()
            {
                Header = (char)header,
                Footer = (char)footer,
                ReceivedAt = DateTime.Now,
                Message = message
            };

            return avionicsInfo;
        }

        private AvionicsError ExtractAvionicsError(List<byte> rawDataList, char header, char footer, int footerIndex)
        {
            var message = System.Text.Encoding.ASCII.GetString(rawDataList.Skip(1).Take(footerIndex - 1).ToArray());

            AvionicsError avionicsError = new()
            {
                Header = (char)header,
                Footer = (char)footer,
                ReceivedAt = DateTime.Now,
                Message = message
            };

            return avionicsError;
        }
    }
}
