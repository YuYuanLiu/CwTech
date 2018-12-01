
#ifndef __header_var_h__
#define __header_var_h__

#include "header.h"

//====================================
// WDT
//====================================
uint8_t wdtTimeout = WDTO_8S;
const uint32_t wdtMaxCount = 100;
const uint32_t nothing_delay = 100;
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
// Serial
//====================================
String serialReadBuffer = String(256);

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



#endif