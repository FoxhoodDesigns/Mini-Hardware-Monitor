//Bare Basics for hardware monitor working

const int ledPin = 13;      // the pin that the LED is attached to
byte Serial_buffer[7];
byte CPU_Load = 0;
byte MEM_Load = 0;
byte GPU_Load = 0;
byte CPU_Temp = 0;
byte GPU_Temp = 0;

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
    Serial.readBytes(Serial_buffer, 7);
    if (Serial_buffer[0] == 0x03 && Serial_buffer[1] == 0xF4) { //Verifies packet as from the Monitor application.
      CPU_Load = Serial_buffer[2];
      MEM_Load = Serial_buffer[3];
      GPU_Load = Serial_buffer[4];
      CPU_Temp = Serial_buffer[5];
      GPU_Temp = Serial_buffer[6];
      analogWrite(ledPin, Sensor_2); //Output Sensor 2
      
    }
  }
}
