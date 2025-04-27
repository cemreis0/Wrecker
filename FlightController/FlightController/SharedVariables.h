#ifndef SHAREDVARIABLES_H
#define SHAREDVARIABLES_H

#include <UARTMessageHandler.h>

// Define general enum for initialization status of modules
enum class InitStatus {
  NotInitialized = 0,
  Initialized = 1
};

// Declare variables to hold status values
extern InitStatus bno055InitStatus;
extern InitStatus bmp280InitStatus;
extern InitStatus gpsInitStatus;
extern InitStatus i2cInitStatus;

// Declare shared sensor data struct
extern SensorData sharedSensorData;

// Declare shared status messages for modules
extern const char* bno055StatusMessage;
extern const char* bmp280StatusMessage;
extern const char* gpsStatusMessage;
extern const char* i2cStatusMessage;  // Wire connection for BNO055 + BMP280

// Declare shared recovery flags
extern bool isRecoveryConditionsMet;
extern bool isRecoverySignalSent;

// Declare LED pin that is used to indicate setup stage
extern const int ledPin;

#endif  // SHAREDVARIABLES_H
