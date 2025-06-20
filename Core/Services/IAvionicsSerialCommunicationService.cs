﻿using Core.Models;
using System.IO.Ports;

namespace Core.Services
{
    public interface IAvionicsSerialCommunicationService
    {
        event EventHandler AvionicsMessageReadyForProcess;

        void Connect(string portName, int baudRate, StopBits stopBits, Parity parityBits, int readTimeout, int writeTimeout);
        void Disconnect();
        Task<AvionicsBase>? ProcessSerialQueue();
    }
}
