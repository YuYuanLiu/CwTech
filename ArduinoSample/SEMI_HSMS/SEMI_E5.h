/*
 SEMI_E5(SEMI II)
 written by : Liu Yu Yuan -> 20161115
 */
#ifndef SEMI_E5_H
#define SEMI_E5_H
#if ARDUINO >= 100
#include "Arduino.h"
#else
#include "WProgram.h"
#endif
// put your main code here,
#define MACHID    "Humidity"
#define SOFTVERS  "VER:1.00"
// SECS DATA TYPES
#define _LIST     0x01
#define _BINARY   0x21
#define _BOOLEAN  0x25
#define _ASCII    0x41
#define _JIS      0x45
#define _CHAR_2   0x49
#define _INT_8    0x61
#define _INT_1    0x65
#define _INT_2    0x69
#define _INT_4    0x71
#define _FT_8     0x81
#define _FT_4     0x91
#define _UINT_1   0xA5
#define _UINT_2   0xA9
#define _UINT_4   0xB1

#define _FORMAT_ERROR_    0xFF
#define TRUE              1
#define FALSE             0

class SEMI_E5 {
private:
	byte *_RecvMsg;
	int *_RecvLen;
	byte *_SendMsg;
	int *_SendLen;
	int32_t buffLoc;

	double **_signals;
	int32_t *_rowNum;
	int32_t *_colNum;

public:
	SEMI_E5(void);
	~SEMI_E5(void);

	void INIT_SEMI_E5(byte *RecvMsg, int *RecvLen, byte *SendMsg, int *SendLen,
			double** signals, int32_t *rowNum, int32_t *colNum);

	bool SECS_COMMAD();

	void process_s1_messages(byte f);
	void output_S1F2(void);
	void output_S1F4(void);
	void output_S9Fx(byte function);

	void init_secs_msg(byte s, byte f, byte wBit);
	void add_list_msg(byte num);
	void add_ascii_msg(byte num, char *msg);
	void add_uint1(byte num, byte *list);
	void add_int4(byte num, byte *list);
	void add_int8(byte num, byte *list);
	void add_ft8(byte num, byte *list);
	void add_binary(byte num, byte *list);
	void common_byte_add(byte num, byte *list);
	void common_byte_add_endian(byte *list, byte num, byte typeSize);
	byte dList_msg(byte *inBlock);
	byte d_ascii_msg(byte *dataBlock);
	byte d_uint2(unsigned int *num, byte *dataBlock);

};
#endif
