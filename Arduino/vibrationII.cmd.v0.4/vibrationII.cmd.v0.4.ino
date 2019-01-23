#ifndef __vibrationII_cmd_v0_4_ino__
#define __vibrationII_cmd_v0_4_ino__

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
  Serial.begin(19200);
  for (int i = 0; i < 5 && !Serial; i++)
  {
    // wait for serial port to connect. Needed for native USB port only
    delay(1000);
  }
  //--- EEPROM --------------------------------------
  g_eeprom.IP[0] = 192;
  g_eeprom.IP[1] = 168;
  g_eeprom.IP[2] = 123;
  g_eeprom.IP[3] = 201;
  CheckEEPROM_READ(&g_eeprom.IP[0], sizeof(g_eeprom));

  //--- Ethernet --------------------------------------
  //Ethernet.begin(mac); //DHCP
  //Ethernet.begin(mac, ip, myDns, gateway, subnet); // Full setting
  Ethernet.begin(mac, ip);
  server.begin();
  if (Serial){
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
  wdt_reset();
  wdtCount++;
  if (wdtCount > wdtMaxCount){
    //無連線delay 100ms * 100次 = 10秒無連線
    delay(16 * 1000);//WDT timeout 最大8秒, 設定16秒以重啟Arduino
  }


  readVibration();
  bool flag = ethComm();
  flag |= serialComm();
    
  if (flag) {
    wdtCount = 0;
  }else{
    delay(nothing_delay);//無連線delay 100ms
  }
  //CheckSerialCMD();
}


/*
  SerialEvent occurs whenever a new data comes in the
 hardware serial RX.  This routine is run between each
 time loop() runs, so using delay inside loop can delay
 response.  Multiple bytes of data may be available.
 */
void serialEvent()
{
  return;
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
