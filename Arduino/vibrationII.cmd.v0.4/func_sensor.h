#ifndef __func_sensor_h__
#define __func_sensor_h__

#include "header.h"




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
	respHumidityToEth();
	respTemperatureToEth();
}

//=======================
// ADS1252 Function
//=======================

byte Read_ADS1252() {
	byte j = 0;
	for (byte i = 0; i < 8; i++) {
		j = j << 1;
		//digitalWrite(ADS_SCLK, HIGH);
		PORTD |= B00000100;
		//if(digitalRead(ADS_DATA) == HIGH) j= j + 1;
		if ((PIND & B00001000) > 0)
			j = j + 1;
		//digitalWrite(ADS_SCLK, LOW);
		PORTD &= B11111011;
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
//		delayMicroseconds(100);   //delete by Liu

	}

}





#endif
