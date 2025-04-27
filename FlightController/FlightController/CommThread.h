#ifndef COMMTHREAD_H
#define COMMTHREAD_H

#include <SDLogHandler.h>
#include <SD.h>
#include <UARTMessageHandler.h>
#include <Arduino_Threads.h>
#include "SharedVariables.h"

// Define the delay between messages and calculations
#define DELAY_DEFAULT_COMM 500 // 500 ms
#define DELAY_ONE_SECOND 1000 // 1000 ms

// Declare external communication modules and handlers
extern UART e32; // UART for E32 LoRa
extern UARTMessageHandler uartMessageHandler; // UARTMessageHandler instance
extern SDLogHandler sdLogHandler; // SDLogHandler instance

// Declare SD card variables and constants
extern const int sdChipSelect;

// Declare flags to track logging
extern bool hasLoggedRecoveryConditions;
extern bool hasLoggedRecoverySignal;

class CommThread : public Arduino_Threads {
    protected:
        virtual void setup() override;
        virtual void loop() override;
};

#endif // COMMTHREAD_H
