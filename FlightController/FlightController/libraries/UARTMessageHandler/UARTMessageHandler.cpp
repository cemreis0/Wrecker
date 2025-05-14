#include "UARTMessageHandler.h"

// Constructor implementation
UARTMessageHandler::UARTMessageHandler(HardwareSerial &serial) : _serial(serial) {}

// Send info message
void UARTMessageHandler::info(const char* message) {
    sendMessage('I', message);
}

// Send error message
void UARTMessageHandler::error(const char* message) {
    sendMessage('E', message);
}

// Send data message
void UARTMessageHandler::data(const SensorData &sensorData) {
    _serial.write('D'); // Prefix for data message
    _serial.print(sensorData.roll); // Roll
    _serial.write(','); // Separator
    _serial.print(sensorData.pitch); // Pitch
    _serial.write(','); // Separator
    _serial.print(sensorData.yaw); // Yaw
    _serial.write(','); // Separator
    _serial.print(sensorData.altitude); // Altitude
    _serial.write(','); // Separator
    _serial.print(sensorData.velocity); // Velocity
    _serial.write(','); // Separator
    _serial.print(sensorData.latitude); // Latitude
    _serial.write(','); // Separator
    _serial.print(sensorData.longitude); // Longitude
    _serial.write(10); // End character
    _serial.flush(); // Ensure the message is fully transmitted
}

// Utility function to send a message with start and end characters
void UARTMessageHandler::sendMessage(char prefix, const char* message) {
    _serial.write(prefix); // Prefix (I or E)
    _serial.print(message); // Message content
    _serial.write(10); // End character
    _serial.flush(); // Ensure the message is fully transmitted
}