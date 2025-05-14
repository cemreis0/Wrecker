#include "SensorThread.h"

// Initialize sensor objects
BNO055 bno055(&Wire, 0x28); // Create BNO055 object
BMP280 bmp280(&Wire, DFRobot_BMP280_IIC::eSdoLow); // Create BMP280 object
UART gps(8, 9); // Instantiate UART for GPS
TinyGPSPlus tinyGPSPlus; // Create TinyGPSPlus object
SimpleKalmanFilter altitudeKalmanFilter(0.2, 1.0, 0.01); // Create SimpleKalmanFilter to estimate altitude

// Initialize variables to calculate linear velocity
float current_altitude = 0.0f;
float previous_altitude = 0.0f;
uint32_t current_pressure = 0;
unsigned long current_time = 0;
unsigned long previous_time = 0;

// Define pin for recovery
const int recoveryPin = 22; // Pin for recovery system
const int recoveryPin2 = 21; // Pin for recovery system 2

// Get BNO055 status and update the status variable
const char* getBNO055Status(BNO055::eStatus_t eStatus) {
    switch(eStatus) {
        case BNO055::eStatusOK:
            bno055InitStatus = InitStatus::Initialized;
            return "BNO055 status OK";
        case BNO055::eStatusErr:
            bno055InitStatus = InitStatus::NotInitialized;
            return "BNO055 unknown error";
        case BNO055::eStatusErrDeviceNotDetect:
            bno055InitStatus = InitStatus::NotInitialized;
            return "BNO055 device not detected";
        case BNO055::eStatusErrDeviceReadyTimeOut:
            bno055InitStatus = InitStatus::NotInitialized;
            return "BNO055 device ready timeout";
        case BNO055::eStatusErrDeviceStatus:
            bno055InitStatus = InitStatus::NotInitialized;
            return "BNO055 device internal status error";
        default:
            bno055InitStatus = InitStatus::NotInitialized;
            return "BNO055 unknown status";
    }
}

// Get BMP280 status and update the status variable
const char* getBMP280Status(BMP280::eStatus_t eStatus) {
    switch(eStatus) {
        case BMP280::eStatusOK:
            bmp280InitStatus = InitStatus::Initialized;
            return "BMP280 status OK";
        case BMP280::eStatusErr:
            bmp280InitStatus = InitStatus::NotInitialized;
            return "BMP280 unknown error";
        case BMP280::eStatusErrDeviceNotDetected:
            bmp280InitStatus = InitStatus::NotInitialized;
            return "BMP280 device not detected";
        case BMP280::eStatusErrParameter:
            bmp280InitStatus = InitStatus::NotInitialized;
            return "BMP280 parameter error";
        default:
            bmp280InitStatus = InitStatus::NotInitialized;
            return "BMP280 unknown status";
    }
}

// Calculate linear velocity using altitude data from BMP280
float calcVelocity(float current_altitude, float previous_altitude, unsigned long current_time, unsigned long previous_time) {
    float deltaAltitude = current_altitude - previous_altitude; // Calculate the change in altitude
    float deltaTime = (current_time - previous_time) / 1000.0f; // Calculate the change in time in milliseconds and convert to seconds
    float velocity = deltaAltitude / deltaTime; // Calculate velocity
    return velocity; // Return velocity in m/s
}

void SensorThread::setup() {
  // Set recovery and led pins to low on initialization and both pins mode to output
  pinMode(ledPin, OUTPUT);
  pinMode(recoveryPin, OUTPUT);
  digitalWrite(ledPin, LOW);
  digitalWrite(recoveryPin, LOW);

  // LED on Pico is in the setup stage
  digitalWrite(ledPin, HIGH);

  // Initialize GPS
  gps.begin(9600);
  delay(DELAY_DEFAULT_SENSOR);
  gpsInitStatus = InitStatus::Initialized;
  gpsStatusMessage = "GPS UART communication established";

  // Initialize I2C bus for BNO055 + BMP280
  Wire.begin();
  delay(DELAY_DEFAULT_SENSOR); // It takes time to initialize the I2C bus
  i2cInitStatus = InitStatus::Initialized;
  i2cStatusMessage = "I2C bus initialized";

  // Initialize BNO055
  bno055.reset();
  while (bno055.begin() != BNO055::eStatusOK) {
      bno055StatusMessage = getBNO055Status(bno055.lastOperateStatus);
      delay(DELAY_ONE_SECOND);
  }
  bno055StatusMessage = getBNO055Status(bno055.lastOperateStatus);

  // Initialize BMP280
  bmp280.reset();
  while (bmp280.begin() != BMP280::eStatusOK) {
      bmp280StatusMessage = getBMP280Status(bmp280.lastOperateStatus);
      delay(DELAY_ONE_SECOND);
  }
  bmp280StatusMessage = getBMP280Status(bmp280.lastOperateStatus);

  // Make initial calculations with data from BNO055 and BMP280 to avoid sending incorrect data to ground
  for (int i = 0; i < NUMBER_OF_INITIAL_MEASUREMENTS; i++) {
      current_pressure = bmp280.getPressure();
      current_altitude = bmp280.calAltitude(SEA_LEVEL_PRESSURE, current_pressure);
      previous_altitude = current_altitude;
      sharedSensorData.altitude = altitudeKalmanFilter.updateEstimate(current_altitude); // filtered altitude
      BNO055::sEulAnalog_t sEulAnalog = bno055.getEul();
      delay(10);
  }

  // Get previous altitude and time as initial readings
  current_pressure = bmp280.getPressure();
  previous_altitude = bmp280.calAltitude(SEA_LEVEL_PRESSURE, current_pressure);
  previous_time = millis();

  // LED on Pico is off when the setup stage is over
  digitalWrite(ledPin, LOW);
}

void SensorThread::loop() {
    // Read pressure (Pa) from BMP280, pass it in to altitude (m) calculation
    current_pressure = bmp280.getPressure();
    current_altitude = bmp280.calAltitude(SEA_LEVEL_PRESSURE, current_pressure);
    sharedSensorData.altitude = altitudeKalmanFilter.updateEstimate(current_altitude); // filtered altitude

    current_time = millis(); // Read current time
    sharedSensorData.velocity = calcVelocity(sharedSensorData.altitude, previous_altitude, current_time, previous_time); // Calculate linear velocity

    // Set current altitude and time as previous when velocity calculation is finished
    previous_altitude = sharedSensorData.altitude;
    previous_time = current_time;

    // Read NMEA sentences
    while (gps.available()) {
        tinyGPSPlus.encode(gps.read());
    }

    // Assign GPS position values if location data is valid
    if (tinyGPSPlus.location.isValid()) {
        sharedSensorData.latitude = tinyGPSPlus.location.lat();
        sharedSensorData.longitude = tinyGPSPlus.location.lng();
    }

    // Read BNO055 Euler angles (roll, pitch, yaw)
    BNO055::sEulAnalog_t sEulAnalog = bno055.getEul();
    sharedSensorData.roll = sEulAnalog.roll;
    sharedSensorData.pitch = sEulAnalog.pitch;
    sharedSensorData.yaw = sEulAnalog.head;

    // Check if recovery conditions are satisfied and recovery signal has not been sent
    if (!isRecoverySignalSent) {
        if(sEulAnalog.pitch < -90 || sEulAnalog.pitch > 90 && sharedSensorData.velocity < 0.50) {
            // Recovery conditions are met
            isRecoveryConditionsMet = true;

            // Set the pins high
            digitalWrite(ledPin, HIGH);
            digitalWrite(recoveryPin, HIGH);

            // Apply the signal for a second
            delay(DELAY_ONE_SECOND);

            // Set the pins low
            digitalWrite(recoveryPin, LOW);
            digitalWrite(ledPin, LOW);

            // Never run this block again
            isRecoverySignalSent = true;
        }
    }

    // Check if recovery conditions are satisfied and recovery signal has not been sent
    if (!isRecoverySignalSent2) {
        // Recovery conditions are met 2
        if (sharedSensorData.altitude < 550)

            // Set the pins high
            digitalWrite(ledPin, HIGH);
            digitalWrite(recoveryPin2, HIGH);

            // Apply the signal for a second
            delay(DELAY_ONE_SECOND);

            // Set the pins low
            digitalWrite(recoveryPin2, LOW);
            digitalWrite(ledPin, LOW);

            // Never run this block again
            isRecoverySignalSent2 = true;
    }
    
    delay(DELAY_DEFAULT_SENSOR);
}
