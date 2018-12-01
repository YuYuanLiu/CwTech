#ifndef __vibrationII_cmd_v0_3_ino__
#define __vibrationII_cmd_v0_3_ino__

#include "header.h"
/*
 * 當前的震動板
 * 大板
 * Update 2017/11/30 , Added 2 functions, (1)R/W EEPROM, (2)GET/SET IP Adderss.
 * Update 2018/07/16 , 修正ADS1252通訊架構, By Liu
 * */

//=======================
// Basic Function
//=======================
void setup()
{
  //--- Serial --------------------------------------
  Serial.begin(9600);
  for (int i = 0; i < 5 && !Serial; i++)
  {
    // wait for serial port to connect. Needed for native USB port only
    delay(1000);
  }
  //--- EEPROM --------------------------------------
  g_eeprom.IP[0] = 1;
  g_eeprom.IP[1] = 1;
  g_eeprom.IP[2] = 1;
  g_eeprom.IP[3] = 1;
  CheckEEPROM_READ(&g_eeprom.IP[0], sizeof(g_eeprom));

  //--- Ethernet --------------------------------------
  //Ethernet.begin(mac); //DHCP
  //Ethernet.begin(mac, ip, myDns, gateway, subnet); // Full setting
  Ethernet.begin(mac, ip);
  server.begin();
  if (Serial)
  {
    Serial.print("Server address:");
    Serial.println(Ethernet.localIP());
  }

  //--- ADS1252 --------------------------------------
  pinMode(ADS_SCLK, OUTPUT);
  pinMode(ADS_DATA, INPUT);
  reset_adc();
  digitalWrite(ADS_SCLK, LOW);
  //--- WDT --------------------------------------
  wdt_enable(wdtTimeout);
  //--- Timer --------------------------------------
}
void loop()
{

  //--- Main --------------------------------------
  //Serial.println("Wait for connecting...");
  EthernetClient client = server.available();
  wdt_reset();
  wdtCount++;
  if (wdtCount > wdtMaxCount)
  {
    Serial.print("Module Reset");
    while (1)
      ;
  }
  if (client)
  {
    wdtCount = 0;
    { //無效工作, 但要留下來, compiler會把無用的code移除
      int zeroCount = 0;
      char readChar;
      while (zeroCount < 1)
      {
        readChar = client.read();
        if (readChar < 0)
          break;
        zeroCount++;
      }
    }
    procReadVibration();
  }
  else
  {
    delay(nothing_delay);
  }

  Ethernet.maintain();

  CheckSerialCMD();
}


/*
  SerialEvent occurs whenever a new data comes in the
 hardware serial RX.  This routine is run between each
 time loop() runs, so using delay inside loop can delay
 response.  Multiple bytes of data may be available.
 */
void serialEvent()
{
  while (Serial.available())
  {
    // get the new byte:
    char inChar = (char)Serial.read();
    recv_buff[recv_len++] = inChar;
    //Serial.print(inChar);
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n')
    {
      stringComplete = true;
    }
  }
}



#endif
