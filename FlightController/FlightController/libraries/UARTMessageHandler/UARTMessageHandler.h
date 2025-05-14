#ifndef UARTMESSAGEHANDLER_H
#define UARTMESSAGEHANDLER_H

#include <Arduino.h>
#include "SensorData.h"

class UARTMessageHandler {
public:
    // Constructor: Initialize with a HardwareSerial reference
    UARTMessageHandler(HardwareSerial &serial);

    // Methods to send messages
    void info(const char* message);
    void error(const char* message);
    void data(const SensorData &sensorData);

private:
    HardwareSerial &_serial;

    // Utility function to send messages with start and end characters
    void sendMessage(char prefix, const char* message);
};

#endif // UARTMESSAGEHANDLER_H