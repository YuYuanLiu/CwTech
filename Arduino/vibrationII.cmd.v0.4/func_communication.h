#ifndef __func_communication_h__
#define __func_communication_h__

#include "header.h"



void respVibToSerial() {
  
    if (!Serial)
        return;
    String respData = "cmd -respData";
    
    respData += " -svid " + String(0);
    respData += " -data ";
    for (int idx = 0; idx < sizeAdsSignals; idx++)
      respData += String(adsSignals[idx]) + " ";
  
    respData += "    \r\n";
    Serial.print(respData);

}


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

void respHumidityToEth() {
	//Serial.println("printHumidityToEth...");
	String respData = "cmd -respData";
	respData += " -svid " + String(1);
	respData += " -data ";
	respData += String(1.0) + " ";

	respData += "    \r\n";
	//Serial.println(respData);
	server.print(respData);
}

void respTemperatureToEth() {
	//Serial.println("printHumidityToEth...");
	String respData = "cmd -respData";
	respData += " -svid " + String(2);
	respData += " -data ";
	respData += String(temperatureC[0]) + " ";

	respData += "    \r\n";

	server.print(respData);
}






bool ethComm()
{
    Ethernet.maintain(); //必加
    EthernetClient client = server.available();

    ethReadBuffer = "";
    char readChar = 0;
    while (client.available())
    {
        readChar = client.read();
        if (readChar < 0)
            continue;
        if (readChar == '\n')
            break;//結束字元
        ethReadBuffer += readChar;

        if (ethReadBuffer.length() > 8)
        {
            //長度超過8, 卻找不到cmd, 就把字串截掉
            if (ethReadBuffer.indexOf("cmd") < 0)
                ethReadBuffer = ethReadBuffer.substring(4);
        }

    }


    //if (ethReadBuffer.indexOf("-reqData") >= 0
      //|| ethReadBuffer.indexOf("-req_data") >= 0)
    if(ethReadBuffer.indexOf("cmd") >= 0)
    {
        respVibToEth();
        return true;
    }

    return false;
}





bool serialComm()
{
    if (!Serial)
        return false;

    serialReadBuffer = "";
    char readChar = 0;
    while (Serial.available())
    {
        readChar = Serial.read();
        if (readChar < 0)
            continue;
        if (readChar == '\n')
            break;
        serialReadBuffer += readChar;

        if (serialReadBuffer.length() > 8)
        {
            //長度超過8, 卻找不到cmd, 就把字串截掉
            if (serialReadBuffer.indexOf("cmd") < 0)
                serialReadBuffer = serialReadBuffer.substring(4);
        }

    }

    //if (serialReadBuffer.indexOf("-reqData") >= 0
    //  || serialReadBuffer.indexOf("-req_data") >= 0)
    if (serialReadBuffer.indexOf("cmd") >= 0)
    {
        respVibToSerial();
        return true;
    }

    return false;
}



#endif
