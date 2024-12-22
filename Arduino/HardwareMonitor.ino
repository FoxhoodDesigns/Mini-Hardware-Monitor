//Bare Basics for hardware monitor working

const int ledPin = 13;      // the pin that the LED is attached to
byte Serial_buffer[11];
byte Sensor_1 = 0;
byte Sensor_2 = 0;
byte Sensor_3 = 0;

void setup() {
  // initialize the serial communication:
  Serial.begin(115200);
  // initialize the ledPin as an output:
  pinMode(ledPin, OUTPUT);
}

void loop() {
  byte brightness;
  
  // check if data has been sent from the computer:
  if (Serial.available()) {
    Serial.readBytes(Serial_buffer, 11);
    if (Serial_buffer[0] == 0x03 && Serial_buffer[1] == 0xF4) { //Verifies packet as from the Monitor application.
      Sensor_1 = Serial_buffer[2];
      Sensor_2 = Serial_buffer[3];
      Sensor_3 = Serial_buffer[4];
      analogWrite(ledPin, Sensor_2); //Output Sensor 2
      
    }
  }
}
