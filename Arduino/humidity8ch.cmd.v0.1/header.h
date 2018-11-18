#ifndef __header_h__
#define __header_h__



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


#endif