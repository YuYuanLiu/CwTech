/*
 * 當前的震動板
 * 大版
 * Update 2017/11/30 , Added 2 functions, (1)R/W EEPROM, (2)GET/SET IP Adderss.
 * */
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
// EEPROM Variable
//=======================
struct dataType{
  unsigned char IP[4];  
  unsigned char CheckSum[2];
};
dataType g_eeprom;

//=======================
// Serial Port Variable
//=======================
String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
byte recv_buff[20] = {0};
int recv_len=0;

//====================================
// WDT
//====================================
uint8_t wdtTimeout = WDTO_8S;
const uint32_t wdtMaxCount = 360;
const uint32_t nothing_delay = 10;
uint32_t wdtCount = 0;

//=======================
// Ethernet
//=======================
byte mac[] = { 0xc0, 0x69, 0x6d, 0x00, 0x00, 0x01 };
IPAddress ip(192, 168, 123, 201);
IPAddress myDns(192, 168, 123, 1);
IPAddress gateway(192, 168, 123, 1);
IPAddress subnet(255, 255, 255, 0);
EthernetServer server(5000);

String ethReadString;

//=======================
// ADS1252 Vibration
//=======================

#define _4Mhz 1
#ifdef _4Mhz
int ADS1252_reset = 2880; // 4 Mhz osc
int ADS1252_Drdy = 54 + 3; // 4 Mhz osc
#elif _16Mhz
		int ADS1252_reset = 720;
		int ADS1252_Drdy = 14 + 3;
#endif

byte ads_b1 = 0;
byte ads_b2 = 0;
byte ads_b3 = 0;
long ads_signal = 0;
int sizeAdsSignals = 4;
long adsSignals[4];

//=======================
// Humidity
//=======================
//--- DHT -------------------------------------------------
#define DHTPIN 22
#define DHTTYPE DHT22   // DHT 22  (AM2302)

const int NumOfDhts = 1;
DHT* dhtObj[NumOfDhts];
int dhtPins[NumOfDhts] = { 2 };

float humidity[NumOfDhts];
float temperatureC[NumOfDhts];
float temperatureF[NumOfDhts];
float heatIndex[NumOfDhts];



//=======================
// Ethernet Function
//=======================
void printVibToEth() {
  //Serial.println("printToEth...");
  String respData = "cmd -respData";
  respData += " -svid " + String(0);
  respData += " -data ";
  for (int idx = 0; idx < sizeAdsSignals; idx++)
    respData += String(adsSignals[idx]) + " ";

  respData += "    \r\n";

  server.println(respData);
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
  server.println(respData);
}

void printTemperatureToEth() {
  //Serial.println("printHumidityToEth...");
  String respData = "cmd -respData";
  respData += " -svid " + String(2);
  respData += " -data ";
  respData += String(temperatureC[0]) + " ";

  respData += "    \r\n";

  server.println(respData);
}

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
  printHumidityToEth();
  printTemperatureToEth();
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
    delayMicroseconds(100);
  }

}


//=======================
// Basic Function
//=======================
void setup() {
	//--- Serial --------------------------------------
  Serial.begin(9600);//可能沒有RS232的連線
  
  //--- EEPROM --------------------------------------
  g_eeprom.IP[0]=1;
  g_eeprom.IP[1]=1;
  g_eeprom.IP[2]=1;
  g_eeprom.IP[3]=1;
  CheckEEPROM_READ(&g_eeprom.IP[0],  sizeof(g_eeprom));
  
  //--- Ethernet --------------------------------------
  //Ethernet.begin(mac); //DHCP
  //Ethernet.begin(mac, ip, myDns, gateway, subnet); // Full setting
  Ethernet.begin(mac, ip);
  server.begin();

  //--- ADS1252 --------------------------------------
  pinMode(ADS_SCLK, OUTPUT);
  pinMode(ADS_DATA, INPUT);
  reset_adc();
  digitalWrite(ADS_SCLK, LOW);
  
	//--- WDT --------------------------------------
	wdt_enable(wdtTimeout);
	//--- Timer --------------------------------------
}
void loop() {

  EthernetClient client = server.available();
  wdt_reset();
  wdtCount++;
  if (wdtCount > wdtMaxCount)
    delay(10 * 1000);
  if (client) {
    //client.connected 也無法判斷是否連線
    //即使連線了, 沒有資料過來也會是false
    wdtCount = 0;
    if(client.available()){
      while (client.available()) client.read();
      readVibration();
      printVibToEth();
    }
    

  } else {
    delay(nothing_delay);
  }

  Ethernet.maintain();

  CheckSerialCMD();
}

//以不影響原本程式的架構上, 新增EEPROM函式
/*
 * Check EEPROM -> Read
 */
void CheckEEPROM_READ(unsigned char* Data, unsigned char DataSize)
{
  //====================================================================
  //讀出來先確認CheckSum是否正確, MCU第一次跑程式一定是0, 之後就正常讀寫.
  int i;
  word TempSum = 0;
  for(i = 0; i < DataSize ; i++)
  {
    *(Data + i) = EEPROM.read(i);
  }
  for(i = 0; i < (DataSize - 2) ; i++)  //CheckSum 是 2 bytes
  {
    TempSum += *(Data + i);
  }
  //====================================================================
  //如果CheckSum不對, 將原本ip寫入eeprom, 反之將eeprom寫入ip.
  if(((TempSum / 256) != *(Data + i)) || ((TempSum % 256) != *(Data + i + 1)))
  {//不相等, 將原本ip寫入g_eeprom
    for(i = 0; i < (DataSize - 2) ; i++)  //CheckSum 是 2 bytes
    {
      *(Data + i) = ip[i];
    }
    CheckEEPROM_WRITE(Data, DataSize);
  }
  else
  {//相等, g_eeprom讀出值寫入ip
    for(i = 0; i < (DataSize - 2) ; i++)  //CheckSum 是 2 bytes
    {
      ip[i] = *(Data + i);
    }
  }
  //====================================================================
  //Serial.println(ip); //For Test
}

/*
 * Check EEPROM -> Write
 */
void CheckEEPROM_WRITE(unsigned char* Data, unsigned char DataSize)
{
  int i;
  word TempSum = 0;
  //====================================================================
  //首先算出CheckSum
  for(i = 0; i < (DataSize - 2) ; i++)  //CheckSum 是 2 bytes
  {
    TempSum += *(Data + i);
  }
  *(Data + i) = (TempSum / 256);
  *(Data + i + 1) = (TempSum % 256);
  //====================================================================
  //開始寫入
  for(i = 0; i < DataSize ; i++)
  {
    EEPROM.write(i, *(Data + i));
  }
  //====================================================================
}

/*
  SerialEvent occurs whenever a new data comes in the
 hardware serial RX.  This routine is run between each
 time loop() runs, so using delay inside loop can delay
 response.  Multiple bytes of data may be available.
 */
void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    recv_buff[recv_len++] = inChar;
    //Serial.print(inChar);
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}

/*
 * CheckSerialCMD
 */
void rt()
{
  if (stringComplete) {
    //Serial.println(recv_len); //For Test
    //Serial.println(inputString);  //For Test
    // 定義命令
    // 1. Get IP Address -> IP?\r\n
    // 2. Set IP Address -> IP192.168.0.1.\r\n
    if(recv_buff[0] == 'I' && recv_buff[1] == 'P' && recv_buff[2] == '?'){
      GetIPAddress();
    }
    else if(recv_buff[0] == 'I' && recv_buff[1] == 'P'){
      SetIPAddress();
    }

    inputString = "";
    stringComplete = false;
    recv_len = 0;
  }
}

/*
 * GetIPAddress
 * Return IP Address via Serial Port
 */
void GetIPAddress()
{
  Serial.print("IP Address : ");
  Serial.print(g_eeprom.IP[0]);
  Serial.print(".");
  Serial.print(g_eeprom.IP[1]);
  Serial.print(".");
  Serial.print(g_eeprom.IP[2]);
  Serial.print(".");
  Serial.println(g_eeprom.IP[3]);
}

/*
 * SetIPAddress
 * Setting IP Address via Serial Port
 */
void SetIPAddress()
{
  //搜尋"." , 而且一定要有4個 , 從IP開始會有4個點, 之間有4組數字, 就是IP Address
  dataType temp_eeprom;
  char Str_result_1, Str_result_2, Str_result_3, Str_result_4;
  //找第 1 個點
  Str_result_1 = inputString.indexOf(".");
  if(Str_result_1 < 0){
    Serial.println("1 Fail"); //For Test
    return;
  }
  //取第 1 組數字
  //確認是否為數字, 避免輸入者亂輸入
  if(!isValidNumber(inputString.substring(2 ,Str_result_1)))
    return;
  //文字轉數字
  temp_eeprom.IP[0] = inputString.substring(2 ,Str_result_1).toInt();
  Serial.println(temp_eeprom.IP[0]);
  //找第 2 個點
  Str_result_2 = inputString.indexOf(".", Str_result_1 + 1);
  if(Str_result_2 < 0){
    Serial.println("2 Fail"); //For Test
    return;
  }
  //取第 2 組數字
  //確認是否為數字, 避免輸入者亂輸入
  if(!isValidNumber(inputString.substring(Str_result_1 + 1 ,Str_result_2)))
    return;
  //文字轉數字
  temp_eeprom.IP[1] = inputString.substring(Str_result_1 + 1 ,Str_result_2).toInt();
  Serial.println(temp_eeprom.IP[1]);
  //找第 3 個點
  Str_result_3 = inputString.indexOf(".", Str_result_2 + 1);
  if(Str_result_3 < 0){
    Serial.println("3 Fail"); //For Test
    return;
  }
  //取第 3 組數字
  //確認是否為數字, 避免輸入者亂輸入
  if(!isValidNumber(inputString.substring(Str_result_2 + 1 ,Str_result_3)))
    return;
  //文字轉數字
  temp_eeprom.IP[2] = inputString.substring(Str_result_2 + 1 ,Str_result_3).toInt();
  Serial.println(temp_eeprom.IP[2]);
  //找第 4 個點
  Str_result_4 = inputString.indexOf(".", Str_result_3 + 1);
  if(Str_result_4 < 0){
    Serial.println("4 Fail"); //For Test
    return;
  }
  //取第 4 組數字
  //確認是否為數字, 避免輸入者亂輸入
  if(!isValidNumber(inputString.substring(Str_result_3 + 1 ,Str_result_4)))
    return;
  //文字轉數字
  temp_eeprom.IP[3] = inputString.substring(Str_result_3 + 1 ,Str_result_4).toInt();
  Serial.println(temp_eeprom.IP[3]);

  g_eeprom = temp_eeprom;
  CheckEEPROM_WRITE(&g_eeprom.IP[0],  sizeof(g_eeprom));
}

/*
 * isValidNumber ? True or False
 */
boolean isValidNumber(String str){
  boolean isNum = false;
  if(str.length() > 3)  //IP每一段不可能超過 3 位數
    return false;
  for(byte i = 0 ; i < str.length() ; i++)
  {
    isNum = isDigit(str.charAt(i));
    if(!isNum) return false;
  }
  return isNum;
}
