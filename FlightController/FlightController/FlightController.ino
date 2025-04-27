#include <Arduino_Threads.h>
#include "SensorThread.h"
#include "CommThread.h"

// Create instances of the threads
SensorThread sensorThread;
CommThread commThread;

void setup() {
    // Initialize the threads
    sensorThread.start();
    commThread.start();
}

void loop() {
    // The thread library handles running the threads' loop methods
}
