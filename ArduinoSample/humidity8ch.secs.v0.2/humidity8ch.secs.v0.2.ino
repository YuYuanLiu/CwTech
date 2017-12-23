#ifndef HUMIDITY8CH_SECS_V0_2_H_
#define HUMIDITY8CH_SECS_V0_2_H_


#include "DHT.h"
//#include "Timer.h"
#include <SPI.h>
#include <Ethernet.h>
#include <avr/wdt.h>

#include "SEMI_HSMS.h"
#include "SEMI_E37.h"

#define DHTPIN 2     // what pin we're connected to
#define DHTTYPE DHT22   // DHT 22  (AM2302)

//====================================
// Pin Definition
//====================================
#define DHTPIN 2     // what pin we're connected to

#define SEL00 5
#define SEL01 4
#define SEL02 3
#define SEL_EN 6

#define INTERLOCK  8
#define RELAY_OUTPUT   9





//====================================
// Timer
//====================================
//Timer t;


//====================================
// WDT
//====================================
uint8_t wdtTimeout = WDTO_8S;
const uint32_t wdtMaxCount = 36000;
uint32_t wdtCount = 0;


//====================================
// Ethernet
//====================================

byte mac[] = { 0xc0, 0x69, 0x6d, 0xff, 0xff, 0x01 };
IPAddress ip(192, 168, 123, 201);
byte myDns[] = { 192,168, 123, 1 };
byte gateway[] = { 192, 168, 123, 1 };
byte subnet[] = { 255, 255, 255, 0 };

EthernetServer server = EthernetServer(5000);



//====================================
// SECS
//====================================
byte recv_buff[RECV_BUFF_MAX]; // = { 0 };
int recv_len = 0;
byte send_buff[SEND_BUFF_MAX]; // = { 0 };
int send_len = 0;

SEMI_E37 SEMIe37;


//====================================
// DHT
//====================================

#define DHTTYPE DHT22   // DHT 22  (AM2302)
const int NumOfDhts = 8;
DHT* dhtObj[NumOfDhts];
int indexOfDths = 0;

float humidity[NumOfDhts];
float temperatureC[NumOfDhts];
float temperatureF[NumOfDhts];
float heatIndex[NumOfDhts];






//====================================
// Print
//====================================

void printToSerial() {

  for (int idx = 0; idx < NumOfDhts; idx++) {
    float loopH = humidity[idx];
    float loopTc = temperatureC[idx];
    float loopTf = temperatureF[idx];



    if (isnan(loopH) || isnan(loopTc)) {
      Serial.print("Failed to read from ");
      Serial.print(idx, DEC);
      Serial.println("th DHT sensor");
    }
    else {

      Serial.print("DHT[");
      Serial.print(idx, DEC);
      Serial.print("] ");
      Serial.print("Humidity: ");
      Serial.print(loopH);
      Serial.print(" %\t");
      Serial.print("Temperature: ");
      Serial.print(loopTc);
      Serial.print(" *C ");
      Serial.print(loopTf);
      Serial.print(" *F\t");
      Serial.print("Heat index: ");
      float loopHi = heatIndex[idx];
      Serial.print(loopHi);
      Serial.println(" *F");
    }
  }
  Serial.println();
}


void printToEth() {
  Serial.println("printToEth...");
  for (int idx = 0; idx < NumOfDhts; idx++) {
    float loopH = humidity[idx];
    float loopTc = temperatureC[idx];
    //float loopTf = temperatureF[idx];



    if (!isnan(loopH) && !isnan(loopTc)) {
      String respData = "cmd -respData";
      respData += " -svid " + String(idx);
      respData += " -data " + String(loopH);
      respData += "    \n";
      server.println(respData);
      Serial.println(respData);
    }
  }


  for (int idx = 0; idx < NumOfDhts; idx++) {
    float loopH = humidity[idx];
    float loopTc = temperatureC[idx];
    //float loopTf = temperatureF[idx];



    if (!isnan(loopH) && !isnan(loopTc)) {
      String respData = "cmd -respData";
      respData += " -svid " + String(idx+10);
      respData += " -data " + String(loopTc);
      respData += "    \n";
      server.println(respData);
      Serial.println(respData);
    }
  }



}






//====================================
// Ethernet
//====================================




void procEth() {



}

void setupEthernet() {
  //Serial.println("Request IP from DHCP server...");
  //if (Ethernet.begin(mac) == 0) {
  //Serial.println("Failed to configure Ethernet using DHCP");
  Serial.println("Fix IP ");
  Ethernet.begin(mac, ip, myDns, gateway, subnet);
  //}
  ip = Ethernet.localIP();
  for (byte thisByte = 0; thisByte < 4; thisByte++) {
    Serial.print(ip[thisByte], DEC);
    Serial.print(".");
  }
  Serial.println();
  server.begin();
}





//====================================
// DHT
//====================================

void Get_DHT22_Data()
{
  // Reading temperature or humidity takes about 250 milliseconds!
  // Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)
  float h = dhtObj[indexOfDths]->readHumidity();
  // Read temperature as Celsius
  float t = dhtObj[indexOfDths]->readTemperature();
  // Read temperature as Fahrenheit
  float f = dhtObj[indexOfDths]->readTemperature(true);

  float hi = dhtObj[indexOfDths]->computeHeatIndex(f, h);

  humidity[indexOfDths] = h;
  temperatureC[indexOfDths] = t;
  temperatureF[indexOfDths] = f;
  heatIndex[indexOfDths] = hi;


}

void SwitchChannel(int Value) {
	if (Value < 0) {  //off  SEL_EN = Hi
		digitalWrite(SEL_EN, HIGH);
	} else {
		digitalWrite(SEL_EN, LOW);
		digitalWrite(SEL00, Value & 0x1 ? HIGH : LOW);
		digitalWrite(SEL01, Value & 0x2 ? HIGH : LOW);
		digitalWrite(SEL02, Value & 0x4 ? HIGH : LOW);
	}
	delay(5);
}




//====================================
// Main Function
//====================================


void setup() {
  Serial.begin(9600);
  while (!Serial); // wait for serial port to connect. Needed for native USB port only

  setupEthernet();


  // put your setup code here, to run once:
  pinMode(SEL00, OUTPUT);
  pinMode(SEL01, OUTPUT);
  pinMode(SEL02, OUTPUT);
  pinMode(SEL_EN, OUTPUT);

  for (int idx = 0; idx < NumOfDhts; idx++) {
    //DHT aDHT(dhtPins[idx], DHTTYPE); //define a new DHT at pin i with type 11;
    dhtObj[idx] = new DHT(DHTPIN, DHTTYPE);
    dhtObj[idx]->begin();
  };


  wdt_enable(wdtTimeout);
  wdtCount = 0;
}

void loop()
{
  indexOfDths = (indexOfDths+1) % NumOfDhts;

  SwitchChannel(indexOfDths);
  Get_DHT22_Data();

  printToSerial();
  procEth();


}




#endif /* HUMIDITY8CH_CMD_V0_1_H_ */