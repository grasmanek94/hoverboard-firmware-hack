#include <SoftwareSerial.h>

SoftwareSerial ss(2,3);

struct SerialCommand
{
   int16_t steer;
   int16_t speed;
} __attribute__ ((packed));

SerialCommand command;

void setup() {
  ss.begin(19200);
  Serial.begin(19200);

  command.steer = 0;
  command.speed = 0;
}

void loop() {
//  bool changed = false;
/*  while(Serial.available())
  {
    char c = Serial.read();
    switch(c)
    {
        case 'Q':
         command.speed += 1;
         changed = true;
         break;
        case 'A':
         command.speed -= 1;
         changed = true;
         break;
        case 'W':
         command.speed += 10;
         changed = true;
         break;
        case 'S':
         command.speed -= 10;
         changed = true;
         break;     
        case 'E':
         command.speed += 100;
         changed = true;
         break;
        case 'D':
         command.speed -= 100;
         changed = true;
         break;    

        case 'Y':
         command.steer += 1;
         changed = true;
         break;
        case 'U':
         command.steer -= 1;
         changed = true;
         break;
        case 'H':
         command.steer += 10;
         changed = true;
         break;
        case 'J':
         command.steer -= 10;
         changed = true;
         break;     
        case 'N':
         command.steer += 100;
         changed = true;
         break;
        case 'M':
         command.steer -= 100;
         changed = true;
         break;  
    }
  }*/

//  if(changed)
  while(Serial.available() > sizeof(command))
  {
    char c;
    do
    {
        c = Serial.read();
        continue;
    }
    while(c != ';');

    while(Serial.available() < sizeof(command))
    {}
    
    Serial.readBytes((char*)&command, sizeof(command));
    Serial.println("Speed: " + String(command.speed) + ", steer: " + String(command.steer));
    ss.write((char*)&command, sizeof(command));
  }
}