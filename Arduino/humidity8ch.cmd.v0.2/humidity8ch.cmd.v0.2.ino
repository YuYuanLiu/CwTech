

#ifndef HUMIDITY8CH_CMD_V0_1_H_
#define HUMIDITY8CH_CMD_V0_1_H_

#include "header.h"




void setup() {
  Serial.begin(9600);
  for(int i = 0 ; i < 5 && !Serial ; i++){
    // wait for serial port to connect. Needed for native USB port only
   delay(1000);
  }

  //--- Ethernet --------------------------------------
  //Ethernet.begin(mac); //DHCP
  //Ethernet.begin(mac, ip, myDns, gateway, subnet); // Full setting
  Ethernet.begin(mac, ip);
  server.begin();
  if(Serial){
    Serial.print("Server address:");
    Serial.println(Ethernet.localIP());
  }

  //---  --------------------------------------

  // put your setup code here, to run once:
  pinMode(SEL00, OUTPUT);
  pinMode(SEL01, OUTPUT);
  pinMode(SEL02, OUTPUT);
  pinMode(SEL_EN, OUTPUT);

  pinMode(INTERLOCK, OUTPUT);
  pinMode(RELAY_OUTPUT, OUTPUT);

  for (int idx = 0; idx < NumOfDhts; idx++) {
    //DHT aDHT(dhtPins[idx], DHTTYPE); //define a new DHT at pin i with type 11;
    dhtObj[idx] = new DHT(DHTPIN, DHTTYPE);
    dhtObj[idx]->begin();
  };

  wdt_enable(wdtTimeout);
  wdtCount = 0;
}

void loop() {
  indexOfDths = (indexOfDths + 1) % NumOfDhts;
  SwitchChannel(indexOfDths);
  Get_DHT22_Data();

  wdt_reset();
  wdtCount++;
  if (wdtCount > wdtMaxCount){
    //無連線delay 100ms * 100次 = 10秒無連線
    delay(16 * 1000);//WDT timeout 最大8秒, 設定16秒以重啟Arduino
  }
    
  bool flag = ethComm();
  flag |= serialComm();
    
  if (flag) {
    wdtCount = 0;
  }else{
    delay(nothing_delay);//無連線delay 100ms
  }

  


  


  float vrValue = analogRead(VR_INPUT) / 1023.0 * 100.0;
  if(Serial){
    //Serial.print("VR: ");
    //Serial.println(vrValue);
  }
  
  if(humidity[0] > vrValue){
    digitalWrite(INTERLOCK,HIGH);
    digitalWrite(RELAY_OUTPUT,HIGH);
  }else{
    digitalWrite(INTERLOCK,LOW);
    digitalWrite(RELAY_OUTPUT,LOW);    
  }
  

}

#endif /* HUMIDITY8CH_CMD_V0_1_H_ */
