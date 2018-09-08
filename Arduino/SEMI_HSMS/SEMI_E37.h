/*
 SEMI_E37(HSMS)
 written by : Liu Yu Yuan -> 20161115
 */
#ifndef SEMI_E37_H
#define SEMI_E37_H
#if ARDUINO >= 100
#include "Arduino.h"
#else
#include "WProgram.h"
#endif
#include "SEMI_E5.h"

#define LEN_INDEX       0
#define MSG_FIX_LEN     4
#define MSG_HEARER_LEN  10
#define DEVICE_ID_INDEX (MSG_FIX_LEN + 0)
#define STREAM_INDEX    (MSG_FIX_LEN + 2)
#define FUNCTION_INDEX  (MSG_FIX_LEN + 3)
//#define HAS_REPLY(y)    ((y&0x80)?(1):(0))  //沒用到
#define PTYPE_INDEX     (MSG_FIX_LEN + 4)
#define STYPE_INDEX     (MSG_FIX_LEN + 5)
#define SWITCH_NUM      (MSG_FIX_LEN + 6)
#define MESSAGE_INDEX   (MSG_FIX_LEN + 10)

class SEMI_E37 {
private:
	SEMI_E5 SEMIe5;
	byte *_RecvMsg;
	int *_RecvLen;
	byte *_SendMsg;
	int *_SendLen;

public:
	double** signals;
	int32_t rowNum;
	int32_t colNum;


public:
	SEMI_E37(void);
	~SEMI_E37(void);

	void INIT_SEMI_E37(byte *RecvMsg, int *RecvLen, byte *SendMsg, int *SendLen,
			double **signals,int32_t rowNum, int32_t colNum);

	bool MSG_Analyze();

	bool GetWBit(void);
	byte GetStream(void);
	byte GetFunction(void);
	int GetRcvLength(void);
	byte GetSType(void);

	int32_t GetMsgLength(void);

};
#endif
