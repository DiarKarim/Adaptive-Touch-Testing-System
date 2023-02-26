// Single Motor Control example sketch
// by Andrew Kramer
// 5/28/2015

// makes the motor run at 1/2 power

#define PWMA 3 // PWM pin for right hand motor
#define AIN1 4 // direction control pin 1
               // AIN1 on the motor controller
#define AIN2 5 // direction control pin 2 
               // AIN2 pin on motor controller

int pwmsig = 100;
int dely = 650;
float motorCurr = 0; 
float voltage  = 0; 

void setup() {

  Serial.begin(9600); 
  
  // set all pins to output
  for (int pin = 3; pin <= 5; pin++) {
    pinMode(pin, OUTPUT); // set pins 3 through 5 to OUTPUT
  }

    // set motor direction to forward 
  digitalWrite(AIN1, HIGH);
  digitalWrite(AIN2, LOW);
  
  // set the motor to run at 1/2 power
  analogWrite(PWMA, pwmsig);
}

void loop() {

  motorCurr = analogRead(A0); 
  voltage = motorCurr * (5.0 / 1024);
  Serial.println(voltage);
  
//  // set motor direction to forward 
//  digitalWrite(AIN1, HIGH);
//  digitalWrite(AIN2, LOW);
//  
//  // set the motor to run at 1/2 power
//  analogWrite(PWMA, pwmsig);

//  delay(dely);
//
//
//  // set motor direction to forward 
//  digitalWrite(AIN1, LOW);
//  digitalWrite(AIN2, HIGH);
//  
//  // set the motor to run at 1/2 power
//  analogWrite(PWMA, pwmsig);
//
//  delay(dely);
}
