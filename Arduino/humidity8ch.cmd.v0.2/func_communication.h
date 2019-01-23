#ifndef __func_communication_h__
#define __func_communication_h__

#include "header.h"

void respToSerial()
{
    if (!Serial)
        return;
    else
        Serial.println("printToEth...");

    for (int idx = 0; idx < NumOfDhts; idx++)
    {
        float loopH = humidity[idx];
        float loopTc = temperatureC[idx];
        //float loopTf = temperatureF[idx];



        if (!isnan(loopH) && !isnan(loopTc))
        {
            String respData = "cmd -respData";
            respData += " -svid " + String(0x00010000 + idx * 0x0100);
            respData += " -data " + String(loopH);
            respData += "  \r\n";
            server.println(respData);
        }
    }

    for (int idx = 0; idx < NumOfDhts; idx++)
    {
        float loopH = humidity[idx];
        float loopTc = temperatureC[idx];
        //float loopTf = temperatureF[idx];

        if (!isnan(loopH) && !isnan(loopTc))
        {
            String respData = "cmd -respData";
            respData += " -svid " + String(0x00020000 + idx * 0x0100);
            respData += " -data " + String(loopTc);
            respData += "  \r\n";
            server.println(respData);
            if (Serial)
                Serial.println(respData);
        }
    }
}

void respToEth()
{
    if (Serial)
        Serial.println("printToEth...");
    for (int idx = 0; idx < NumOfDhts; idx++)
    {
        float loopH = humidity[idx];
        float loopTc = temperatureC[idx];
        //float loopTf = temperatureF[idx];

        if (!isnan(loopH) && !isnan(loopTc))
        {
            String respData = "cmd -respData";
            respData += " -svid " + String(0x00010000 + idx * 0x0100);
            respData += " -data " + String(loopH);
            respData += "  \r\n";
            server.println(respData);
        }
    }

    for (int idx = 0; idx < NumOfDhts; idx++)
    {
        float loopH = humidity[idx];
        float loopTc = temperatureC[idx];
        //float loopTf = temperatureF[idx];


        if (!isnan(loopH) && !isnan(loopTc))
        {
            String respData = "cmd -respData";
            respData += " -svid " + String(0x00020000 + idx * 0x0100);
            respData += " -data " + String(loopTc);
            respData += "  \r\n";
            server.println(respData);
            if (Serial)
                Serial.println(respData);
        }
    }
}

bool ethComm()
{
    Ethernet.maintain(); //必加
    EthernetClient client = server.available();
    

    ethReadBuffer = "";
    int zeroCount = 0;
    char readChar = 0;
    while (client.available())
    {

        wdtCount = 0;
        readChar = client.read();
        if (readChar < 0)
            continue;
        if (readChar == '\n')
            break;
        ethReadBuffer += readChar;

        if (ethReadBuffer.length() > 8)
        {
            //長度超過8, 卻找不到cmd, 就把字串截掉
            if (ethReadBuffer.indexOf("cmd") < 0)
                ethReadBuffer = ethReadBuffer.substring(4);
        }

        zeroCount++;
    }


    if (ethReadBuffer.indexOf("-reqData") >= 0
      || ethReadBuffer.indexOf("-req_data") >= 0)
    {
        respToEth();
        return true;
    }
    return false;    
}

bool serialComm(){
    if (!Serial)
        return false;

    serialReadBuffer = "";
    int zeroCount = 0;
    char readChar = 0;
    while (Serial.available())
    {
        wdtCount = 0;
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

        zeroCount++;
    }

    if (serialReadBuffer.indexOf("-reqData") >= 0
      || serialReadBuffer.indexOf("-req_data") >= 0)
    {
        respToSerial();
        return true;
    }
    return false;
    
}

#endif
