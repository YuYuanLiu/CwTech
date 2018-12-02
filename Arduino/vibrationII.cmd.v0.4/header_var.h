#ifndef __header_var_h__
#define __header_var_h__

#include "header.h"


//=======================
// EEPROM Variable
//=======================
struct dataType{
  unsigned char IP[4];  
  unsigned char CheckSum[2];
};
dataType g_eeprom;



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

String ethReadBuffer = String(256);

//=======================
// Serial Port Variable
//=======================
String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
byte recv_buff[128] = {0};
int recv_len=0;

String serialReadBuffer = String(256);


//=======================
// ADS1252 Vibration
//=======================
// 1 DRDY partition = (24 + 6 + 6)* p_MCLK = 36 * p_MCLK
// 1 DOUT partition = 348 * p_MCLK
// 1 CONVCYCLE = 1 DRDY partition + 1 DOUT partition = 384 * p_MCLK
// RESET => 5 * CONVCYCLE = 1920 * p_MCLK

#define _4Mhz 1
#ifdef _4Mhz
// CLK for ADS1252 = 4MHz , p_MCLK = 1/4M = 0.25us = 250ns
int ADS1252_reset = 480; // 4 Mhz osc
int ADS1252_Drdy = 10; // 4 Mhz osc
#elif _16Mhz
		int ADS1252_reset = 120;
		int ADS1252_Drdy = 2;
#endif

byte ads_b1 = 0;
byte ads_b2 = 0;
byte ads_b3 = 0;
long ads_signal = 0;
#define sizeAdsSignals  80  //4, 50
long adsSignals[sizeAdsSignals];
//char Temp_Char[34 + 8*sizeAdsSignals] ={0}; //長度計算公式28 + 6 + 8*sizeAdsSignals

//=======================
// Humidity - DHT
//=======================


const int NumOfDhts = 1;
DHT* dhtObj[NumOfDhts];
int dhtPins[NumOfDhts] = { 2 };

float humidity[NumOfDhts];
float temperatureC[NumOfDhts];
float temperatureF[NumOfDhts];
float heatIndex[NumOfDhts];




#endif
