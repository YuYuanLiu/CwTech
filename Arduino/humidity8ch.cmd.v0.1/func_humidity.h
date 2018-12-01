#ifndef __func_humidity_h__
#define __func_humidity_h__

#include "header.h"




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




#endif 