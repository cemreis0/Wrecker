using Core.Enums;
using Core.Models;
using Core.Services;
using GroundConsole.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Services;
using System;
using System.IO.Ports;

namespace GroundConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set up serial port configuration
            SerialPort serialPort = new SerialPort();

            // Change these values according to your setup
            serialPort.PortName = "COM9"; // e.g., "COM3" on Windows or "/dev/ttyUSB0" on Linux
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            serialPort.ReadTimeout = 500; // in milliseconds
            serialPort.WriteTimeout = 500;

            // Event handler for data received
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            try
            {
                serialPort.Open();
                Console.WriteLine("Listening on " + serialPort.PortName + "...");
                Console.WriteLine("Press Ctrl+C to exit.");

                // Keep the program running and listening indefinitely
                while (true)
                {
                    // This ensures the program keeps running
                    // without blocking or exiting.
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening serial port: " + ex.Message);
            }
            finally
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    Console.WriteLine("Serial port closed.");
                }
            }
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                string data = sp.ReadExisting();
                Console.Write(data); // Print data without adding extra newline
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading from serial port: " + ex.Message);
            }
        }
    }
}
