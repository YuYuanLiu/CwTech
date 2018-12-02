#ifndef __header_h__
#define __header_h__


//3th Library
#include <EEPROM.h>
#include <SPI.h>
#include <Ethernet.h>
#include <avr/wdt.h>
#include "DHT.h"





//=======================
// Pin Definition
//=======================

#define ADS_DATA 3
#define ADS_SCLK 2
#define INTERLOCK 8


//=======================
// DHT Definition
//=======================


#define DHTPIN 22
#define DHTTYPE DHT22   // DHT 22  (AM2302)



//Project Library
#include "header_var.h"
#include "func_communication.h"
#include "func_sensor.h"
#include "func_eeprom.h"


#endif