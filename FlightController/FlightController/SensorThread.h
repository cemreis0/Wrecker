#ifndef SENSORTHREAD_H
#define SENSORTHREAD_H

#include <SimpleKalmanFilter.h>
#include <TinyGPSPlus.h>
#include <DFRobot_BMP280.h>
#include <DFRobot_BNO055.h>
#include <Wire.h>
#include <Arduino_Threads.h>
#include "SharedVariables.h"

// Define the delay between messages and calculations
#define DELAY_DEFAULT_SENSOR 50 // ms
#define DELAY_ONE_SECOND 1000 // ms

// Define the number of initial measurements for BNO055 and BMP280
#define NUMBER_OF_INITIAL_MEASUREMENTS 500 // Take this many initial measurements every 10 ms

// Sea level pressure (Pa)
#define SEA_LEVEL_PRESSURE 1013.25f

// Use aliases instead of full names
typedef DFRobot_BNO055_IIC BNO055;
typedef DFRobot_BMP280_IIC BMP280;

// Declare external sensor, module, and handler objects
extern BNO055 bno055;
extern BMP280 bmp280;
extern UART gps;
extern TinyGPSPlus tinyGPSPlus;
extern SimpleKalmanFilter altitudeKalmanFilter;

// Declare variables to calculate linear velocity
extern float current_altitude;
extern float previous_altitude;
extern uint32_t current_pressure;
extern unsigned long current_time;
extern unsigned long previous_time;

// Declare pin for recovery system
extern const int recoveryPin;

class SensorThread : public Arduino_Threads {
    protected:
        virtual void setup() override;
        virtual void loop() override;
};

#endif // SENSORTHREAD_H
