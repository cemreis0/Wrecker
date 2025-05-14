#ifndef SENSOR_DATA_H
#define SENSOR_DATA_H

// This struct defines the sensor data format
struct SensorData {
  float roll;
  float pitch;
  float yaw;
  float altitude;
  float velocity;
  float latitude;
  float longitude;
};

#endif // SENSOR_DATA_H