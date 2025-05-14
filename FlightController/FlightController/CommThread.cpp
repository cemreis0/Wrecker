#include "CommThread.h"

// Initialize communication modules and handlers
UART e32(0, 1); // Instantiate UART for E32 LoRa
UARTMessageHandler uartMessageHandler(e32); // Instantiate UARTMessageHandler with E32
SDLogHandler sdLogHandler("log.txt"); // Instantiate SDLogHandler with a filename

// Initialize SD card variables and constants
const int sdChipSelect = 17;

// Initialize flags to track if logging has been done
bool hasLoggedRecoveryConditions = false;
bool hasLoggedRecoverySignal = false;

void CommThread::setup() {
    // Set recovery pin to low on initialization and its pin mode to output
    pinMode(ledPin, OUTPUT);
    digitalWrite(ledPin, LOW);

    // LED on Pico is on in the setup stage
    digitalWrite(ledPin, HIGH);

    // Initialize E32
    while (!e32) {
      e32.begin(9600);
    }   
    uartMessageHandler.info("E32 UART communication established");
    sdLogHandler.info("E32 UART communication established");

    // Initialize SD card
    if (!SD.begin(sdChipSelect)) {
        uartMessageHandler.error("Failed to initialize SD Card");
        sdLogHandler.error("Failed to initialize SD Card");
        delay(DELAY_ONE_SECOND);
    }
    uartMessageHandler.info("SD card initialized");
    sdLogHandler.info("SD card initialized");

    // Communicate the GPS status, it should establish communication once it's started
    while (gpsInitStatus != InitStatus::Initialized) {
      uartMessageHandler.error(gpsStatusMessage);
      sdLogHandler.error(gpsStatusMessage);
      delay(DELAY_ONE_SECOND);
    }
    uartMessageHandler.info(gpsStatusMessage);
    sdLogHandler.info(gpsStatusMessage);

    // Communicate the I2C (BNO055 + BMP280) status, it should establish communication once it's started
    while (i2cInitStatus != InitStatus::Initialized) {
      uartMessageHandler.error(i2cStatusMessage);
      sdLogHandler.error(i2cStatusMessage);
      delay(DELAY_ONE_SECOND);
    }
    uartMessageHandler.info(i2cStatusMessage);
    sdLogHandler.info(i2cStatusMessage);

    // Communicate the BNO055 status, it should start after a while
    while (bno055InitStatus != InitStatus::Initialized) {
        uartMessageHandler.error(bno055StatusMessage);
        sdLogHandler.error(bno055StatusMessage);
        delay(DELAY_ONE_SECOND);
    }
    uartMessageHandler.info(bno055StatusMessage);
    sdLogHandler.info(bno055StatusMessage);

    // Communicate the BMP280 status, it should start after a while
    while (bmp280InitStatus != InitStatus::Initialized) {
        uartMessageHandler.error(bmp280StatusMessage);
        sdLogHandler.error(bmp280StatusMessage);
        delay(DELAY_ONE_SECOND);
    }
    uartMessageHandler.info(bmp280StatusMessage);
    sdLogHandler.info(bmp280StatusMessage);

    // Led on Pico is off when the setup stage is over
    digitalWrite(ledPin, LOW);
}

void CommThread::loop() {
    // Log the recovery condition once when it is met
    if (isRecoveryConditionsMet && !hasLoggedRecoveryConditions) {
      uartMessageHandler.info("Recovery conditions met");
      sdLogHandler.info("Recovery conditions met");
      hasLoggedRecoveryConditions = true; // Prevent further logging
      delay(50);
    }

    // Log the recovery signal once when it is sent
    if (isRecoverySignalSent && !hasLoggedRecoverySignal) {
      uartMessageHandler.info("Recovery signal sent");
      sdLogHandler.info("Recovery signal sent");
      hasLoggedRecoverySignal = true; // Prevent further logging
      delay(50);
    }

    // Send data in the format: <D: roll, pitch, yaw, altitude, velocity, latitude, longitude>
    uartMessageHandler.data(sharedSensorData);
    sdLogHandler.data(sharedSensorData.roll, sharedSensorData.pitch, sharedSensorData.yaw, sharedSensorData.altitude, sharedSensorData.velocity, sharedSensorData.latitude, sharedSensorData.longitude);

    // Delay is disabled
    // delay(DELAY_DEFAULT_COMM);
}
