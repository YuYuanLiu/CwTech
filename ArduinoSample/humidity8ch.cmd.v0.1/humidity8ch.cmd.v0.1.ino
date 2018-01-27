/*
 * 後來用的8ch
 *
 */

#ifndef HUMIDITY8CH_CMD_V0_1_H_
#define HUMIDITY8CH_CMD_V0_1_H_

#include "DHT.h"
#include <SPI.h>
#include <Ethernet.h>
#include <avr/wdt.h>

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

#define VR_INPUT A0

//====================================
// Timer
//====================================
//Timer t;

//====================================
// WDT
//====================================
uint8_t wdtTimeout = WDTO_8S;
const uint32_t wdtMaxCount = 1000;
const uint32_t nothing_delay = 10;
uint32_t wdtCount = 0;


//====================================
// Ethernet
//====================================

byte mac[] = { 0xc0, 0x69, 0x6d, 0xff, 0xff, 0x01 };
IPAddress ip(192, 168, 123, 201);
byte myDns[] = { 192, 168, 123, 1 };
byte gateway[] = { 192, 168, 123, 1 };
byte subnet[] = { 255, 255, 255, 0 };

EthernetServer server = EthernetServer(5000);

String ethReadBuffer = String(256);

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
  if(!Serial)return;
	for (int idx = 0; idx < NumOfDhts; idx++) {
		float loopH = humidity[idx];
		float loopTc = temperatureC[idx];
		float loopTf = temperatureF[idx];

		if (isnan(loopH) || isnan(loopTc)) {
			Serial.print("Failed to read from ");
			Serial.print(idx, DEC);
			Serial.println("th DHT sensor");
		} else {

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
  if(Serial)
	  Serial.println("printToEth...");
	for (int idx = 0; idx < NumOfDhts; idx++) {
		float loopH = humidity[idx];
		float loopTc = temperatureC[idx];
		//float loopTf = temperatureF[idx];

		if (!isnan(loopH) && !isnan(loopTc)) {
			String respData = "cmd -respData";
			respData += " -svid " + String(0x00010000 + idx * 0x0100);
			respData += " -data " + String(loopH);
			respData += "    \r\n";
			server.println(respData);
      if(Serial)
			  Serial.println(respData);
		}
	}

	for (int idx = 0; idx < NumOfDhts; idx++) {
		float loopH = humidity[idx];
		float loopTc = temperatureC[idx];
		//float loopTf = temperatureF[idx];

		if (!isnan(loopH) && !isnan(loopTc)) {
			String respData = "cmd -respData";
			respData += " -svid " + String(0x00020000 + idx * 0x0100);
			respData += " -data " + String(loopTc);
			respData += "    \r\n";
			server.println(respData);
      if(Serial)
			  Serial.println(respData);
		}
	}

}


//====================================
// DHT
//====================================

void Get_DHT22_Data() {
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
  for(int i = 0 ; i < 5 && !Serial ; i++){
    // wait for serial port to connect. Needed for native USB port only
   delay(1000);
  }

  //--- Ethernet --------------------------------------
  //Ethernet.begin(mac); //DHCP
  //Ethernet.begin(mac, ip, myDns, gateway, subnet); // Full setting
  Ethernet.begin(mac, ip);
  server.begin();
  if(Serial){
    Serial.print("Server address:");
    Serial.println(Ethernet.localIP());
  }

  //---  --------------------------------------

	// put your setup code here, to run once:
	pinMode(SEL00, OUTPUT);
	pinMode(SEL01, OUTPUT);
	pinMode(SEL02, OUTPUT);
	pinMode(SEL_EN, OUTPUT);

  pinMode(INTERLOCK, OUTPUT);
  pinMode(RELAY_OUTPUT, OUTPUT);

	for (int idx = 0; idx < NumOfDhts; idx++) {
		//DHT aDHT(dhtPins[idx], DHTTYPE); //define a new DHT at pin i with type 11;
		dhtObj[idx] = new DHT(DHTPIN, DHTTYPE);
		dhtObj[idx]->begin();
	};

	wdt_enable(wdtTimeout);
	wdtCount = 0;
}

void loop() {
	indexOfDths = (indexOfDths + 1) % NumOfDhts;

	SwitchChannel(indexOfDths);
	Get_DHT22_Data();

	printToSerial();
	


  EthernetClient client = server.available();
  wdt_reset();
  wdtCount++;
  if (wdtCount > wdtMaxCount)
    delay(16 * 1000);
  if (client) {
    wdtCount = 0;
    ethReadBuffer = "";
    int zeroCount = 0;
    char readChar = 0;
    while (client.available()) {
      readChar = client.read();
      if (readChar < 0)
        continue;
      if (readChar == '\n')
        break;
      ethReadBuffer += readChar;

      if (ethReadBuffer.length() > 8) {
        if (ethReadBuffer.indexOf("cmd") < 0)
          ethReadBuffer = ethReadBuffer.substring(4);
      }

      zeroCount++;
    }

    if (ethReadBuffer.indexOf("-reqData") >= 0) {
      printToEth();
      wdt_reset();
    }

  } else {
    delay(nothing_delay);
  }

  Ethernet.maintain(); //必加


  


  float vrValue = analogRead(VR_INPUT) / 1023.0 * 100.0;
  if(Serial){
    Serial.print("VR: ");
    Serial.println(vrValue);
  }
  
  if(humidity[0] > vrValue){
    digitalWrite(INTERLOCK,HIGH);
    digitalWrite(RELAY_OUTPUT,HIGH);
  }else{
    digitalWrite(INTERLOCK,LOW);
    digitalWrite(RELAY_OUTPUT,LOW);    
  }
  

}

#endif /* HUMIDITY8CH_CMD_V0_1_H_ */
