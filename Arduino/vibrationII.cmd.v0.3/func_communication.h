#ifndef __func_communication_h__
#define __func_communication_h__

#include "header.h"



//=======================
// Ethernet Function
//=======================
void respVibToEth() {
	//Serial.println("printToEth...");
	String respData = "cmd -respData";
  
	respData += " -svid " + String(0);
	respData += " -data ";
	for (int idx = 0; idx < sizeAdsSignals; idx++)
		respData += String(adsSignals[idx]) + " ";

	respData += "    \r\n";
  server.print(respData);

  //strcpy(Temp_Char, respData.c_str());  //string transfer to char
  //server.write(Temp_Char, sizeof(Temp_Char));
	//printToSerial();
}

void printHumidityToEth() {
	//Serial.println("printHumidityToEth...");
	String respData = "cmd -respData";
	respData += " -svid " + String(1);
	respData += " -data ";
	respData += String(1.0) + " ";

	respData += "    \r\n";
	//Serial.println(respData);
	server.print(respData);
}

void printTemperatureToEth() {
	//Serial.println("printHumidityToEth...");
	String respData = "cmd -respData";
	respData += " -svid " + String(2);
	respData += " -data ";
	respData += String(temperatureC[0]) + " ";

	respData += "    \r\n";

	server.print(respData);
}



void respVibToSerial() {
  
    if (!Serial)
        return;
    else
        Serial.println("printToEth...");

    String respData = "cmd -respData";
    
    respData += " -svid " + String(0);
    respData += " -data ";
    for (int idx = 0; idx < sizeAdsSignals; idx++)
      respData += String(adsSignals[idx]) + " ";
  
    respData += "    \r\n";
    Serial.print(respData);

}




#endif
