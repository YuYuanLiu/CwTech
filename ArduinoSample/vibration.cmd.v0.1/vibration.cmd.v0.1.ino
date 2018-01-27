/*
 * 當前的震動板
 * */

#ifndef VIBRATION_CMD_V0_1_INO_
#define VIBRATION_CMD_V0_1_INO_

#include <SPI.h>
#include <Ethernet.h>
#include <avr/wdt.h>
#include "DHT.h"
//=======================
// Pin Definition
//=======================

#define ADS_DATA 6
#define ADS_SCLK 7
#define INTERLOCK 8

//====================================
// WDT
//====================================
uint8_t wdtTimeout = WDTO_8S;
const uint32_t wdtMaxCount = 1000;
const uint32_t nothing_delay = 10;
uint32_t wdtCount = 0;

//=======================
// Ethernet
//=======================
byte mac[] = { 0xc0, 0x69, 0x6d, 0x00, 0x00, 0x01 };
IPAddress ip(192, 168, 123, 201);
IPAddress myDns(192, 168, 123, 1);
IPAddress gateway(192, 168, 123, 1);
IPAddress subnet(255, 255, 255, 0);
EthernetServer server(5000);

String ethReadString;

//=======================
// ADS1252 Vibration
//=======================

#define _4Mhz 1
#ifdef _4Mhz
int ADS1252_reset = 2880; // 4 Mhz osc
int ADS1252_Drdy = 54 + 3; // 4 Mhz osc
#elif _16Mhz
		int ADS1252_reset = 720;
		int ADS1252_Drdy = 14 + 3;
#endif

byte ads_b1 = 0;
byte ads_b2 = 0;
byte ads_b3 = 0;
long ads_signal = 0;
int sizeAdsSignals = 4;
long adsSignals[4];

//=======================
// Humidity
//=======================
//--- DHT -------------------------------------------------
#define DHTPIN 22
#define DHTTYPE DHT22   // DHT 22  (AM2302)

const int NumOfDhts = 1;
DHT* dhtObj[NumOfDhts];
int dhtPins[NumOfDhts] = { 2 };

float humidity[NumOfDhts];
float temperatureC[NumOfDhts];
float temperatureF[NumOfDhts];
float heatIndex[NumOfDhts];

//=======================
// Ethernet Function
//=======================
void printVibToEth() {
	//Serial.println("printToEth...");
	String respData = "cmd -respData";
	respData += " -svid " + String(0);
	respData += " -data ";
	for (int idx = 0; idx < sizeAdsSignals; idx++)
		respData += String(adsSignals[idx]) + " ";

	respData += "    \n";

	server.println(respData);
	//printToSerial();
}

void printHumidityToEth() {
	//Serial.println("printHumidityToEth...");
	String respData = "cmd -respData";
	respData += " -svid " + String(1);
	respData += " -data ";
	respData += String(1.0) + " ";

	respData += "    \n";
	//Serial.println(respData);
	server.println(respData);
}

void printTemperatureToEth() {
	//Serial.println("printHumidityToEth...");
	String respData = "cmd -respData";
	respData += " -svid " + String(2);
	respData += " -data ";
	respData += String(temperatureC[0]) + " ";

	respData += "    \n";

	server.println(respData);
}

//=======================
// Humidity
//=======================

void readHumidity() {
	for (int idx = 0; idx < NumOfDhts; idx++) {
		humidity[idx] = dhtObj[idx]->readHumidity();
		temperatureC[idx] = dhtObj[idx]->readTemperature(); // Read temperature as Celsius
		temperatureF[idx] = dhtObj[idx]->readTemperature(true); // Read temperature as Fahrenheit
		heatIndex[idx] = dhtObj[idx]->computeHeatIndex(temperatureF[idx],
				humidity[idx]);
	}
}

void procReadHumidity() {
	readHumidity();
	printHumidityToEth();
	printTemperatureToEth();
}

//=======================
// ADS1252 Function
//=======================

byte Read_ADS1252() {
	byte j = 0;
	for (byte i = 0; i < 8; i++) {
		j = j << 1;
		//digitalWrite(ADS_SCLK, HIGH);
		PORTD |= B10000000;
		//if(digitalRead(ADS_DATA) == HIGH) j= j + 1;
		if ((PIND & B01000000) > 0)
			j = j + 1;
		//digitalWrite(ADS_SCLK, LOW);
		PORTD &= B01111111;
	}
	return j;
}

void reset_adc() {
	digitalWrite(ADS_SCLK, HIGH); //Liu+20160510
	delayMicroseconds(ADS1252_reset);
}
void drdy_wait() {
	delayMicroseconds(ADS1252_Drdy);
}

void readVibrationSingle() {
	drdy_wait();
	ads_b1 = Read_ADS1252();
	ads_b2 = Read_ADS1252();
	ads_b3 = Read_ADS1252();

	ads_signal = ads_b1;
	ads_signal <<= 8;
	ads_signal += ads_b2;
	ads_signal <<= 8;
	ads_signal += ads_b3;
}

void readVibration() {
	for (int idx = 0; idx < sizeAdsSignals; idx++) {
		long time = millis();
		while (digitalRead(ADS_DATA) != HIGH) {
			if (millis() - time > 5)
				break;
		}
		readVibrationSingle();
		adsSignals[idx] = ads_signal;
		delayMicroseconds(100);
	}

}

void procReadVibration() {
	readVibration();
	printVibToEth();
}

//=======================
// Basic Function
//=======================
void setup() {
	//--- Serial --------------------------------------
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

	//--- ADS1252 --------------------------------------
	pinMode(ADS_SCLK, OUTPUT);
	pinMode(ADS_DATA, INPUT);
	reset_adc();
	digitalWrite(ADS_SCLK, LOW);
	//--- WDT --------------------------------------
	wdt_enable(wdtTimeout);
	//--- Timer --------------------------------------
}
void loop() {

	//--- Main --------------------------------------
	//Serial.println("Wait for connecting...");
	EthernetClient client = server.available();
	wdt_reset();
	wdtCount++;
	if (wdtCount > wdtMaxCount)
		delay(10 * 1000);
	if (client) {
		wdtCount = 0;
    while (client.read() >= 0) ;
		procReadVibration();
	} else {
		delay(nothing_delay);
	}

	Ethernet.maintain();

}

#endif /* VIBRATION_CMD_V0_1_INO_ */
