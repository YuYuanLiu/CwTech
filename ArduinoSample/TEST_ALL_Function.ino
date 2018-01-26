#include "DHT.h"
#include <Wire.h>

#define DHTPIN 2     // what pin we're connected to
#define DHTTYPE DHT22   // DHT 22  (AM2302)
#define SEL00 5
#define SEL01 4
#define SEL02 3
#define SEL_EN 6
#define SENSOR_NUM 8

#define INTERLOCK  8
#define RELAY_OUTPUT   9

//humidity
DHT dht(DHTPIN, DHTTYPE);
bool Start_Flag = 0;
byte ChannelNUM = 0;
int sensorPin = A0;    // select the input pin for the potentiometer
int sensorValue = 0;  // variable to store the value coming from the sensor

String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete

byte recv_buff[50] = {0};
int recv_len=0;

int sensorVal = 0;

void setup() {
  // initialize serial:
  Serial.begin(115200);
  // reserve 200 bytes for the inputString:
  inputString.reserve(200);
  pinMode(10, OUTPUT);
  digitalWrite(10, HIGH);   //　RS485 enable send
  Serial.println("Hello Nano(Main Board)");
  Serial.flush();          // RS485 wait send finish
  digitalWrite(10, LOW);    //RS485 enable receive
  
  pinMode(INTERLOCK, OUTPUT);  //Interlock
  pinMode(RELAY_OUTPUT, OUTPUT);  //relay output
  digitalWrite(INTERLOCK, HIGH);
  digitalWrite(RELAY_OUTPUT, HIGH);
  
  //timer1 interrupt function
  Init_Timer_1();
  
  //DHT
  pinMode(SEL00, OUTPUT);
  pinMode(SEL01, OUTPUT);
  pinMode(SEL02, OUTPUT);
  pinMode(SEL_EN, OUTPUT);
  
  //DAC
  Wire.begin(); // join i2c bus (address optional for master)
  
  //INPUT
  pinMode(7, INPUT_PULLUP);
  sensorVal = digitalRead(7);
}

void loop() {
  // print the string when a newline arrives:
  if (stringComplete) {
    digitalWrite(10, HIGH);
    Serial.println(inputString);
    Serial.flush();
    digitalWrite(10, LOW);
    // clear the string:
    inputString = "";
    stringComplete = false;
    
    if(recv_buff[0] == 'I' && recv_buff[1] == 'O'){
      if(recv_buff[2] == '1'){
        if(recv_buff[3] == '1'){
          //turn on
          digitalWrite(INTERLOCK, HIGH);
          Serial.println("INTERLOCK HIGH");
        }
        else{
          //tune off
          digitalWrite(INTERLOCK, LOW);
          Serial.println("INTERLOCK LOW");
        }
      }
      else{
        if(recv_buff[3] == '1'){
          //turn on
          digitalWrite(RELAY_OUTPUT, HIGH);
          Serial.println("RELAY_OUTPUT HIGH");
        }
        else{
          //tuen off
          digitalWrite(RELAY_OUTPUT, LOW);
          Serial.println("RELAY_OUTPUT LOW");
        }
      }
    }
    else if((recv_buff[0] == 'D' && recv_buff[1] == 'H' && recv_buff[2] == 'T')){
      if(recv_buff[3] == '1'){
        Start_Flag = 1;
      }
      else{
        Start_Flag = 0;
      }
    }
    else if((recv_buff[0] == 'A' && recv_buff[1] == 'I')){
      if(recv_buff[2] == '0'){
        Anaolg_IN();
      }
    }
    else if((recv_buff[0] == 'D' && recv_buff[1] == 'A' && recv_buff[2] == 'C')){
      //byte Channel_Temp = recv_buff[3] - 0x30;
      //int Value_Temp = ((recv_buff[4]- 0x30) * 1000) + ((recv_buff[5] - 0x30) * 100) + ((recv_buff[6] - 0x30) * 10) + (recv_buff[7] - 0x30);
      //Value_Temp = map(Value_Temp, 0, 5000, 0, 4095);
      //Set_Analog_Output(Channel_Temp, Value_Temp);
      Set_Analog_Output();
      //Serial.print(Channel_Temp);  //for test
      //Serial.print(Value_Temp);    //for test
    } 
    recv_len = 0;
  }
  
  
  if (digitalRead(7) != sensorVal) {
    sensorVal = digitalRead(7);
    if(sensorVal == HIGH) {
      Serial.println("INPUT : HIGH");
    }
    else {
      Serial.println("INPUT : LOW");
    }
  }
}

/*
  SerialEvent occurs whenever a new data comes in the
 hardware serial RX.  This routine is run between each
 time loop() runs, so using delay inside loop can delay
 response.  Multiple bytes of data may be available.
 */
void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    recv_buff[recv_len++] = inChar;
    //Serial.print(inChar);
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}
/*//////////////////////////////
CS12  CS11  CS10
  0     0     0   => no clock source
  0     0     1   => CLK / 1 => 16MHz
  0     1     0   => CLK / 8 => 16M / 8
  0     1     1   => CLK / 64
  1     0     0   => CLK / 256
  1     0     1   => CLK / 1024
*///////////////////////////////
void Init_Timer_1(void){
  TCCR1A = 0x00;                // Normal mode, just as a Timer
  // see datasheet for detail
  TCCR1B |= _BV(CS12);          
  TCCR1B &= ~_BV(CS11);  
  TCCR1B |= _BV(CS10);   
  TIMSK1 |= _BV(TOIE1);         // enable timer overflow interrupt
  TCNT1 = -15625;               // Ticks for 1 second @16 MHz,prescale=64
}
ISR (TIMER1_OVF_vect)
{ 
//  PORTB ^= _BV(5);              // Toggle LED, PB5 = Arduino pin 13 
//  PORTD ^= _BV(7);              // Toggle LED, PD7 = Arduino pin 7
  TCNT1 = -15625;               // Ticks for 1 second @16 MHz,prescale=64
  
  if(Start_Flag == 1){
    if(ChannelNUM++ >= SENSOR_NUM){
      ChannelNUM = 0;
      return;
    }
    SwitchChannel(ChannelNUM);
    Serial.print("Sensor : ");
    Serial.println(ChannelNUM);
    Get_DHT22_Data();
  }
}

void Get_DHT22_Data() {
  float h = dht.readHumidity();
  // Check if any reads failed and exit early (to try again).
  if (isnan(h)) {
    Serial.println("Failed to read from DHT sensor!");
    return;
  }
  else{
    Serial.print("Humidity: "); 
    Serial.print(h);
    Serial.print(" %\t");
  }
}

void SwitchChannel(unsigned char Value)
{
  if(Value == 0)
  {//off  SEL_EN = Hi
    digitalWrite(SEL_EN, HIGH); 
  }
  else if(Value == 1)
  {//   SEL_EN = Lo, SEL02 = Lo, SEL01 = Lo, SEL00 = Lo
    digitalWrite(SEL_EN, LOW);
    digitalWrite(SEL00, LOW); 
    digitalWrite(SEL01, LOW); 
    digitalWrite(SEL02, LOW); 
  }
  else if(Value == 2)
  {//   SEL_EN = Lo, SEL02 = Lo, SEL01 = Lo, SEL00 = Hi
    digitalWrite(SEL_EN, LOW); 
    digitalWrite(SEL00, HIGH); 
    digitalWrite(SEL01, LOW); 
    digitalWrite(SEL02, LOW); 
  }
  else if(Value == 3)
  {//   SEL_EN = Lo, SEL02 = Lo, SEL01 = Hi, SEL00 = Lo
    digitalWrite(SEL_EN, LOW);
    digitalWrite(SEL00, LOW); 
    digitalWrite(SEL01, HIGH); 
    digitalWrite(SEL02, LOW); 
  }
  else if(Value == 4)
  {//   SEL_EN = Lo, SEL02 = Lo, SEL01 = Hi, SEL00 = Hi
    digitalWrite(SEL_EN, LOW);  
    digitalWrite(SEL00, HIGH); 
    digitalWrite(SEL01, HIGH); 
    digitalWrite(SEL02, LOW); 
  }
  else if(Value == 5)
  {//   SEL_EN = Lo, SEL02 = Hi, SEL01 = Lo, SEL00 = Lo
    digitalWrite(SEL_EN, LOW);
    digitalWrite(SEL00, LOW); 
    digitalWrite(SEL01, LOW); 
    digitalWrite(SEL02, HIGH); 
  }
  else if(Value == 6)
  {//   SEL_EN = Lo, SEL02 = Hi, SEL01 = Lo, SEL00 = Hi
    digitalWrite(SEL_EN, LOW); 
    digitalWrite(SEL00, HIGH); 
    digitalWrite(SEL01, LOW); 
    digitalWrite(SEL02, HIGH); 
  }
  else if(Value == 7)
  {//   SEL_EN = Lo, SEL02 = Hi, SEL01 = Hi, SEL00 = Lo
    digitalWrite(SEL_EN, LOW);
    digitalWrite(SEL00, LOW); 
    digitalWrite(SEL01, HIGH); 
    digitalWrite(SEL02, HIGH); 
  }
  else if(Value == 8)
  {//   SEL_EN = Lo, SEL02 = Hi, SEL01 = Hi, SEL00 = Hi
    digitalWrite(SEL_EN, LOW); 
    digitalWrite(SEL00, HIGH); 
    digitalWrite(SEL01, HIGH); 
    digitalWrite(SEL02, HIGH); 
  }
  delay(1);
}

void Anaolg_IN(void){
  sensorValue = analogRead(sensorPin);
  sensorValue = (int)((float)((long)sensorValue * 4750) / 1024);
  Serial.print("Analog Input : ");
  Serial.print(sensorValue);
  Serial.println(" mV");
}

//void Set_Analog_Output(byte Channel, int Value){
void Set_Analog_Output(){
  byte send_buff[20] = {0};
  for(int i =0; i < recv_len ; i++)
    send_buff[i] = recv_buff[i];
  Wire.beginTransmission(1);
  Wire.write(send_buff, recv_len);
  Wire.endTransmission();
/*
  Wire.beginTransmission(1); // transmit to device #1
  Wire.write("DAC");        
  Wire.print(Channel);        
  Wire.print(Value);
  Wire.write("\n");
  Wire.endTransmission();    // stop transmitting
  */
}
