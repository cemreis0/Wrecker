#include "SharedVariables.h"

// Define stati of modules
InitStatus bno055InitStatus = InitStatus::NotInitialized;
InitStatus bmp280InitStatus = InitStatus::NotInitialized;
InitStatus gpsInitStatus = InitStatus::NotInitialized;
InitStatus i2cInitStatus = InitStatus::NotInitialized;

// Create the instance of SensorData with initial values
SensorData sharedSensorData = {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0};

// Define shared status messages for modules
const char* bno055StatusMessage = "BNO055 not initialized";
const char* bmp280StatusMessage = "BMP280 not initialized";
const char* gpsStatusMessage = "GPS UART communication not initialized";
const char* i2cStatusMessage = "I2C bus not initialized"; // Wire connection for BNO055 + BMP280

// Define shared recovery flags
bool isRecoveryConditionsMet = false;
bool isRecoverySignalSent = false;
bool isRecoveryConditionsMet2 = false;
bool isRecoverySignalSent2 = false;

// Define LED pin that is used to indicate setup stage
const int ledPin = 25; // LED pin on Pico
