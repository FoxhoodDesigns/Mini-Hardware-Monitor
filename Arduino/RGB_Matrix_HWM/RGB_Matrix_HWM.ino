#include <Adafruit_Protomatter.h>
//NOTE: This example is coded with a RP2040 board in mind (regular or W)

//Matrix Variables and entity
uint8_t rgbPins[]  = {7, 8, 9, 10, 11, 12};
uint8_t addrPins[] = {2, 3, 4, 5};
uint8_t clockPin   = 13;
uint8_t latchPin   = 1;
uint8_t oePin      = 0;
Adafruit_Protomatter matrix(
  32,          // Width of matrix (or matrix chain) in pixels
  6,           // Bit depth, 1-6
  1, rgbPins,  // # of matrix chains, array of 6 RGB pins for each
  4, addrPins, // # of address pins (height is inferred), array of pins
  clockPin, latchPin, oePin, // Other matrix control pins
  true);      //Enables buffer



//HWM Variables
uint8_t CPU_L = 130;
uint8_t CPU_T = 120;
uint8_t GPU_L = 60;
uint8_t GPU_T = 100;
uint8_t RAM_L = 160;
uint8_t MB_T = 0;
uint8_t bar_height = 23;//Colours
const uint16_t frame_colour = matrix.color565(50, 50, 50);
const uint16_t CPU_colour = matrix.color565(50, 0, 0);
const uint16_t RAM_colour = matrix.color565(50, 50, 0);
const uint16_t GPU_colour = matrix.color565(0, 0, 50);
const uint16_t TER_colour = matrix.color565(50, 5, 5);
const uint16_t TER2_colour = matrix.color565(50, 10, 10);
uint8_t Serial_buffer[8];


//Main render Code
void HWM_loop(){
  if (Serial.available()) {
    Serial.readBytes(Serial_buffer, 7);
    if (Serial_buffer[0] == 0x03 && Serial_buffer[1] == 0xF4) { //Verifies packet as from the Monitor application.
      CPU_L = Serial_buffer[2];
      RAM_L = Serial_buffer[3];
      GPU_L = Serial_buffer[4];
      CPU_T = Serial_buffer[5];
      GPU_T = Serial_buffer[6];
      MB_T = Serial_buffer[7];
      //Store current values:
    }
  }
  //Base Shape
  matrix.fillRect(0,0,32,32,0);
  matrix.drawRect(1,1,5,25,frame_colour);
  matrix.drawRect(7,1,5,25,frame_colour);
  matrix.drawRect(13,1,5,25,frame_colour);
  matrix.drawRect(19,1,5,25,frame_colour);
  matrix.drawRect(25,1,5,25,frame_colour);
  matrix.drawPixel(30, 23,frame_colour);
  matrix.drawPixel(30, 21,frame_colour);
  matrix.drawPixel(30, 19,frame_colour);
  matrix.drawPixel(30, 17,frame_colour);
  matrix.drawPixel(30, 15,frame_colour);
  matrix.drawPixel(30, 13,frame_colour);
  matrix.drawPixel(30, 11,frame_colour);
  matrix.drawPixel(30, 9,frame_colour);
  matrix.drawPixel(30, 7,frame_colour);
  matrix.drawPixel(30, 5,frame_colour);
  matrix.drawPixel(30, 3,frame_colour);
  //Temps bottom
  matrix.drawFastVLine(20, 27, 3, TER_colour);
  matrix.drawFastVLine(26, 27, 3, TER_colour);
  matrix.drawPixel(20, 30,TER2_colour);
  matrix.drawPixel(26, 30,TER2_colour);
  matrix.drawRect(22,29,2,2,CPU_colour);
  matrix.drawRect(28,29,2,2,GPU_colour);
  //Bottom Details
  matrix.drawFastHLine(3, 27, 2, CPU_colour);
  matrix.drawFastVLine(2, 28, 2, CPU_colour);
  matrix.drawFastHLine(3, 30, 2, CPU_colour);
  matrix.drawFastVLine(7, 27, 4, RAM_colour);
  matrix.drawFastVLine(11, 27, 4, RAM_colour);
  matrix.drawPixel(8, 28,RAM_colour);
  matrix.drawPixel(9, 29,RAM_colour);
  matrix.drawPixel(10, 28,RAM_colour);
  matrix.drawFastHLine(15, 27, 2, GPU_colour);
  matrix.drawFastVLine(14, 28, 2, GPU_colour);
  matrix.drawFastHLine(15, 30, 2, GPU_colour);
  matrix.drawPixel(16, 29,GPU_colour);
  //Convert the images
  uint8_t bar_length;
  bar_length = map(CPU_L,0,200,0,bar_height);
  matrix.fillRect(2, 2 + bar_height-bar_length, 3, bar_length, CPU_colour);
  bar_length = map(RAM_L,0,200,0,bar_height);
  matrix.fillRect(8, 2 + bar_height-bar_length, 3, bar_length, RAM_colour);
  bar_length = map(GPU_L,0,200,0,bar_height);
  matrix.fillRect(14, 2 + bar_height-bar_length, 3, bar_length, GPU_colour);
  bar_length = map(CPU_T,0,240,0,bar_height);
  matrix.drawFastVLine(21, 2 + bar_height-bar_length, bar_length, TER_colour);
  bar_length = map(GPU_T,0,240,0,bar_height);
  matrix.drawFastVLine(27, 2 + bar_height-bar_length, bar_length, TER_colour);
  //Update
  matrix.show();
}



void setup() {
  // put your setup code here, to run once:
//Start Serial and Matrix
  Serial.begin(115200);
  ProtomatterStatus status = matrix.begin();
  Serial.print("Protomatter begin() status: ");
  Serial.println((int)status);
  if(status != PROTOMATTER_OK) {  //Halt if fails
    for(;;);
  }
}

void loop() {
  // put your main code here, to run repeatedly:
  HWM_loop();
}
