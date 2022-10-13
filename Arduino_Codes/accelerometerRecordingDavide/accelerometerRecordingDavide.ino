/*
Accelerometer module ADXL335
*/

const int xpin = A0;                  // x-axis of the accelerometer
const int ypin = A1;                  // y-axis
const int zpin = A2;                  // z-axis 
long xvalue;
long yvalue;
long zvalue;

unsigned long oldtime;
unsigned long ttime;

//float roll;
//float pitch;

void setup() {
  // initialize the serial communications:
  Serial.begin(115200);
}

// note: multiply times 9.80665 to get values in m/s/s instead of g
void loop() {
  ttime = float(millis());
  //Serial.print(ttime); //prints time since program started
  //Serial.print("\t"); 

  // read and calibrate and print x-axis
  xvalue = analogRead(xpin); // read raw values
  long x = map(xvalue, 285, 427,- 100,100); // maps the extreme ends analog values between -100 and 100
  //String xg = String(x/(-100.00)); // convert mapped values into acceleration in g
  String xg = String(x/(-100.00)*9.80665); // m/s^2
  Serial.print(xg); 
  //Serial.print(","); 
  Serial.print("\t"); 

  // read and calibrate and print y-axis
  yvalue = analogRead(ypin); 
  long y = map(yvalue, 280, 423, -100, 100); 
  //String yg = String(y/(-100.00));
  String yg = String(y/(-100.00)*9.80665);
  Serial.print(yg);
  //Serial.print(","); 
  Serial.print("\t"); 

  // read and calibrate and print z-axis
  zvalue = analogRead(zpin);
  long z = map(zvalue, 305, 447, -100, 100);
  //String zg = String(z/(100.00));
  String zg = String(z/(100.00)*9.80665);
  //String data = xg + " " + yg + " " + zg;
  Serial.println(zg);

 

  // Calculate Roll and Pitch (rotation around x-axis and y-axis)

//  roll = atan(yg / sqrt(pow(xg, 2) + pow(zg, 2))) * 180 / PI;
//  pitch = atan(-1 * xg / sqrt(pow(yg, 2) + pow(zg, 2))) * 180 / PI;
//  
//  Serial.print(" roll: ");
//  Serial.print(roll);
//  Serial.print(" / ");
//  Serial.print(" pitch: ");
//  Serial.println(pitch);


}
