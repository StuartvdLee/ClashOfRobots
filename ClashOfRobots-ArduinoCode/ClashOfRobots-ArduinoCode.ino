/*
ENGINE POWERS
IN1 -> 0V en IN2 -> 5V: Motor 1 draait ene kant
IN1 -> 5V en IN2 -> 0V: Motor 1 draait andere kant
IN3 -> 0V en IN4 -> 5V: Motor 2 draait ene kant
IN3 -> 5V en IN4 -> 0V: Motor 2 draait andere kant
*/

#include <SoftwareSerial.h>
SoftwareSerial bluetooth(8, 7); // RX, TX  

  int pinMotor1IN1 = 9;
  int pinMotor1IN2 = 10;
  int pinMotor2IN1 = 11;
  int pinMotor2IN2 = 12;
  int pinSwitch = 2;

  bool switchVal = false;
  bool isDebouncing = false;
  unsigned long lastDebounceTime = 0;
  unsigned long debounceDelay = 50;

  bool dead = false;

void setup() {
  //Init Motor 1
  pinMode(pinMotor1IN1, OUTPUT);
  pinMode(pinMotor1IN2, OUTPUT);
  //Init Motor 2
  pinMode(pinMotor2IN1, OUTPUT);
  pinMode(pinMotor2IN2, OUTPUT);

  pinMode(pinSwitch, INPUT);
  pinMode(LED_BUILTIN, OUTPUT);

  Serial.begin(9600);
  bluetooth.begin(9600);

  digitalWrite(LED_BUILTIN, LOW);
}

void loop() {
  if(dead == false) {
    ReadBluetooth();
    ReadSwitch();
  }
}

void ReadSwitch () {
  bool curSwitchVal = digitalRead(pinSwitch);
  if(switchVal != curSwitchVal) {
    if(isDebouncing == false) {
      lastDebounceTime = millis();
      isDebouncing = true;
    }
    if((millis() - lastDebounceTime) > debounceDelay) {
             ControlSwitch();
    }
  }
  else {
    isDebouncing = false;
  }

}
void ControlSwitch () {
  switchVal = digitalRead(pinSwitch);
  if(switchVal == HIGH) {
    // Serial.println("Switch Pressed");
    if(dead == false)
      Kill();
  }
  else {
    //Serial.println("Switch Released");
  }
}

void Kill () {
  if(dead == true)
    return;
  dead = true;
  ControlMotors('s');
  WriteBluetooth('q');
  digitalWrite(LED_BUILTIN, HIGH);
}

void ReadBluetooth () {
  char input;
  if (bluetooth.available() > 0) {
    input = bluetooth.read();  
    Serial.println("Got input: "+input);
    ControlMotors(input);
  } 
}

void WriteBluetooth (char command) {
  bluetooth.write(command);
}

void ControlMotors (char command) {
     char commandChar = command;
     switch(command) {
      case 'f':
          MotorForward();
          break;
        case 'l':
          MotorLeft();
          break;
        case 'r':
          MotorRight();
          break;
        case 'b':
          MotorBackwards();
          break;
        case 's':
          MotorStop();
     }
}

//Engine Functions
void MotorForward () {
  EngageMotor1(1);
  EngageMotor2(1);  
}
void MotorStop () {
  EngageMotor1(0);
  EngageMotor2(0);
}
void MotorLeft () {
  EngageMotor1(0);
  EngageMotor2(1);  
}
void MotorRight () {
  EngageMotor1(1);
  EngageMotor2(0);  
}
void MotorBackwards() {
  EngageMotor1(-1);
  EngageMotor2(-1);
}
void EngageMotor1 (int dir) {
  if(dir == 1) {
    digitalWrite(pinMotor1IN1, HIGH);
    digitalWrite(pinMotor1IN2, LOW);
  }
  else if(dir == -1) {
    digitalWrite(pinMotor1IN1, LOW);
    digitalWrite(pinMotor1IN2, HIGH);
  }
  else {
    digitalWrite(pinMotor1IN1, LOW);
    digitalWrite(pinMotor1IN2, LOW);
  }
}
void EngageMotor2 (int dir) {
  if(dir == 1) {
    digitalWrite(pinMotor2IN1, HIGH);
    digitalWrite(pinMotor2IN2, LOW);
  }
  else if(dir == -1) {
    digitalWrite(pinMotor2IN1, LOW);
    digitalWrite(pinMotor2IN2, HIGH);
  }
  else {
    digitalWrite(pinMotor2IN1, LOW);
    digitalWrite(pinMotor2IN2, LOW);
  }
}


