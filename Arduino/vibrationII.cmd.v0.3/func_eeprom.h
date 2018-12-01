#ifndef __func_eeprom_h__
#define __func_eeprom_h__

#include "header.h"




/*
 * Check EEPROM -> Write
 */
void CheckEEPROM_WRITE(unsigned char *Data, unsigned char DataSize)
{
  int i;
  word TempSum = 0;
  //====================================================================
  //首先算出CheckSum
  for (i = 0; i < (DataSize - 2); i++) //CheckSum 是 2 bytes
  {
    TempSum += *(Data + i);
  }
  *(Data + i) = (TempSum / 256);
  *(Data + i + 1) = (TempSum % 256);
  //====================================================================
  //開始寫入
  for (i = 0; i < DataSize; i++)
  {
    EEPROM.write(i, *(Data + i));
  }
  //====================================================================
}





/*
 * Check EEPROM -> Read
 */
void CheckEEPROM_READ(unsigned char *Data, unsigned char DataSize)
{
  //====================================================================
  //讀出來先確認CheckSum是否正確, MCU第一次跑程式一定是0, 之後就正常讀寫.
  int i = 0;
  word TempSum = 0;
  for (i = 0; i < DataSize; i++)
  {
    *(Data + i) = EEPROM.read(i);
  }
  for (i = 0; i < (DataSize - 2); i++) //CheckSum 是 2 bytes
  {
    TempSum += *(Data + i);
  }
  //====================================================================
  //如果CheckSum不對, 將原本ip寫入eeprom, 反之將eeprom寫入ip.
  if (((TempSum / 256) != *(Data + i)) || ((TempSum % 256) != *(Data + i + 1)))
  {                                      //不相等, 將原本ip寫入g_eeprom
    for (i = 0; i < (DataSize - 2); i++) //CheckSum 是 2 bytes
    {
      *(Data + i) = ip[i];
    }
    CheckEEPROM_WRITE(Data, DataSize);
  }
  else
  {                                      //相等, g_eeprom讀出值寫入ip
    for (i = 0; i < (DataSize - 2); i++) //CheckSum 是 2 bytes
    {
      ip[i] = *(Data + i);
    }
  }
  //====================================================================
  //Serial.println(ip); //For Test
}




/*
 * isValidNumber ? True or False
 */
boolean isValidNumber(String str)
{
  boolean isNum = false;
  if (str.length() > 3) //IP每一段不可能超過 3 位數
    return false;
  for (byte i = 0; i < str.length(); i++)
  {
    isNum = isDigit(str.charAt(i));
    if (!isNum)
      return false;
  }
  return isNum;
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
  if (Str_result_1 < 0)
  {
    //Serial.println("1 Fail"); //For Test
    return;
  }
  //取第 1 組數字
  //確認是否為數字, 避免輸入者亂輸入
  if (!isValidNumber(inputString.substring(2, Str_result_1)))
    return;
  //文字轉數字
  temp_eeprom.IP[0] = inputString.substring(2, Str_result_1).toInt();
  //Serial.println(temp_eeprom.IP[0]);
  //找第 2 個點
  Str_result_2 = inputString.indexOf(".", Str_result_1 + 1);
  if (Str_result_2 < 0)
  {
    //Serial.println("2 Fail"); //For Test
    return;
  }
  //取第 2 組數字
  //確認是否為數字, 避免輸入者亂輸入
  if (!isValidNumber(inputString.substring(Str_result_1 + 1, Str_result_2)))
    return;
  //文字轉數字
  temp_eeprom.IP[1] = inputString.substring(Str_result_1 + 1, Str_result_2).toInt();
  //Serial.println(temp_eeprom.IP[1]);
  //找第 3 個點
  Str_result_3 = inputString.indexOf(".", Str_result_2 + 1);
  if (Str_result_3 < 0)
  {
    //Serial.println("3 Fail"); //For Test
    return;
  }
  //取第 3 組數字
  //確認是否為數字, 避免輸入者亂輸入
  if (!isValidNumber(inputString.substring(Str_result_2 + 1, Str_result_3)))
    return;
  //文字轉數字
  temp_eeprom.IP[2] = inputString.substring(Str_result_2 + 1, Str_result_3).toInt();
  //Serial.println(temp_eeprom.IP[2]);
  //找第 4 個點
  Str_result_4 = inputString.indexOf(".", Str_result_3 + 1);
  if (Str_result_4 < 0)
  {
    //Serial.println("4 Fail"); //For Test
    return;
  }
  //取第 4 組數字
  //確認是否為數字, 避免輸入者亂輸入
  if (!isValidNumber(inputString.substring(Str_result_3 + 1, Str_result_4)))
    return;
  //文字轉數字
  temp_eeprom.IP[3] = inputString.substring(Str_result_3 + 1, Str_result_4).toInt();
  //Serial.println(temp_eeprom.IP[3]);

  g_eeprom = temp_eeprom;
  CheckEEPROM_WRITE(&g_eeprom.IP[0], sizeof(g_eeprom));
}



/*
 * CheckSerialCMD
 */
void CheckSerialCMD()
{
  if (stringComplete)
  {
    //Serial.println(recv_len); //For Test
    //Serial.println(inputString);  //For Test
    // 定義命令
    // 1. Get IP Address -> IP?\r\n
    // 2. Set IP Address -> IP192.168.0.1.\r\n
    if (recv_buff[0] == 'I' && recv_buff[1] == 'P' && recv_buff[2] == '?')
    {
      GetIPAddress();
    }
    else if (recv_buff[0] == 'I' && recv_buff[1] == 'P')
    {
      SetIPAddress();
    }

    inputString = "";
    stringComplete = false;
    recv_len = 0;
  }
}




#endif
