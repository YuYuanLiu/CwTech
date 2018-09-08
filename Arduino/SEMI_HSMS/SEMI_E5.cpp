/*
 SEMI_E5(SEMI II)
 developer : Liu Yu Yuan
 Data : 20161115
 */
#include "SEMI_E5.h"
#include "SEMI_E37.h"

SEMI_E5::SEMI_E5(void) {
	this->_RecvLen = NULL;
	this->_RecvMsg = NULL;
	this->_SendLen = NULL;
	this->_SendMsg = NULL;
	this->buffLoc = 0;

	this->_signals = NULL;
	this->_rowNum = NULL;
	this->_colNum = NULL;
}
SEMI_E5::~SEMI_E5(void) {
}
void SEMI_E5::INIT_SEMI_E5(byte *RecvMsg, int *RecvLen, byte *SendMsg,
		int *SendLen, double** signals, int32_t *rowNum, int32_t *colNum) {
	_RecvMsg = RecvMsg;
	_RecvLen = RecvLen;
	_SendMsg = SendMsg;
	_SendLen = SendLen;

	_signals = signals;
	_rowNum = rowNum;
	_colNum = colNum;

}

bool SEMI_E5::SECS_COMMAD() {
	unsigned char msgType;
	msgType = _RecvMsg[STREAM_INDEX] & 0x7F;
	switch (msgType) {
	case 1:
		process_s1_messages(_RecvMsg[FUNCTION_INDEX]);
		break;
//    case ??:
//      process_s18_messages();
//      break;
	default:
		output_S9Fx(3);
		break;
	}

	// send length
	*_SendLen = buffLoc;
	//not inculde 4 bytes => Length bytes
	buffLoc -= MSG_FIX_LEN;
	_SendMsg[3] = buffLoc & 0x00FF;
	_SendMsg[2] = (buffLoc >> 8) & 0x00FF;
	_SendMsg[1] = (buffLoc >> 16) & 0x00FF;
	_SendMsg[0] = (buffLoc >> 24) & 0x00FF;

	return true;
}

/*************************************************************************/
//  Process S1 Messages
/*************************************************************************/
void SEMI_E5::process_s1_messages(byte f) {
	switch ((int) f) {
	case 1:
		output_S1F2();
		break;
	case 3:
		output_S1F4();
		break;
	default:
		output_S9Fx(5);
		break;
	}
}

/*************************************************************************/
//  output_S1F2.
/*************************************************************************/
void SEMI_E5::output_S1F2(void) {
	//long MgsLen = _RecvMsg[LEN_INDEX] * 0x10000 + _RecvMsg[LEN_INDEX + 1] * 0x100 + _RecvMsg[LEN_INDEX + 2] * 0x100 + _RecvMsg[LEN_INDEX + 3];
	/*if(MgsLen != 10)
	 {
	 output_S9Fx(7);
	 return;
	 }*/
	init_secs_msg(1, 2, FALSE);
	add_list_msg(2);
	add_ascii_msg(strlen(MACHID), (char*) MACHID);
	add_ascii_msg(strlen(SOFTVERS), (char*) SOFTVERS);

}

/*************************************************************************/
//  output_S1F3.
/*************************************************************************/
void SEMI_E5::output_S1F4(void) {
	int32_t MgsLen = _RecvMsg[LEN_INDEX] * 0x10000
			+ _RecvMsg[LEN_INDEX + 1] * 0x100 + _RecvMsg[LEN_INDEX + 2] * 0x100
			+ _RecvMsg[LEN_INDEX + 3];

	const int32_t svidPos = 18;
	int32_t svid = _RecvMsg[svidPos] * 0x10000 + _RecvMsg[svidPos + 1] * 0x100
			+ _RecvMsg[svidPos + 2] * 0x100 + _RecvMsg[svidPos + 3];

	int32_t svidType = svid / 0x100;
	int32_t svidVal = svid % 0x100;
	if (svidType > *this->_rowNum)
		return;
	if (svidVal > *this->_colNum)
		return;
	double val = this->_signals[svidType][svidVal];


	this->init_secs_msg(1, 4, FALSE);
	this->add_list_msg(2);
	this->add_int4(1, (byte*) &svid);
	this->add_ft8(1, (byte*) &val);


	Serial.print("Debug.S1F4=");
	Serial.print(buffLoc, 10);
	Serial.print(" ; ");
	Serial.println("");
}

/*************************************************************************/
//  output_S9Fx.
/*************************************************************************/
void SEMI_E5::output_S9Fx(byte function) {
	init_secs_msg(9, function, FALSE);
	add_binary(10, &_RecvMsg[MSG_FIX_LEN]);
}

/*************************************************************************/
//  Initialize SECS Message Out.
/*************************************************************************/
void SEMI_E5::init_secs_msg(byte s, byte f, byte wBit) {
	int i;
	long SystemBytes;
	buffLoc = MSG_FIX_LEN;
// inti S, F bytes, Upper and Lower Block Numbers
	_SendMsg[buffLoc + 0] = 0;
	_SendMsg[buffLoc + 1] = 1;
	_SendMsg[buffLoc + 2] = s;
	if (wBit)
		_SendMsg[buffLoc + 2] |= 0x80;
	_SendMsg[buffLoc + 3] = f;
	_SendMsg[buffLoc + 4] = 0;
	_SendMsg[buffLoc + 5] = 0;

	SystemBytes = 0;
	for (i = 0; i < 4; i++) {
		SystemBytes <<= 8;
		SystemBytes += _RecvMsg[SWITCH_NUM + i];
	}
// Message Serial Number
	if (f & 0x01)
		SystemBytes++;
	_SendMsg[buffLoc + 9] = SystemBytes & 0x00FF;
	_SendMsg[buffLoc + 8] = (SystemBytes >> 8) & 0x00FF;
	_SendMsg[buffLoc + 7] = (SystemBytes >> 16) & 0x00FF;
	_SendMsg[buffLoc + 6] = (SystemBytes >> 24) & 0x00FF;

	buffLoc = MESSAGE_INDEX;
}
/*************************************************************************/
//  (encoder)Add units to output messages.
/*************************************************************************/
void SEMI_E5::add_list_msg(byte num) {
	_SendMsg[buffLoc] = _LIST;
	_SendMsg[buffLoc + 1] = num;
	buffLoc += 2;
}
void SEMI_E5::add_ascii_msg(byte num, char *msg) {
	_SendMsg[buffLoc] = _ASCII;
	common_byte_add(num, (byte *) msg);
}
void SEMI_E5::add_uint1(byte num, byte *list) {
	_SendMsg[buffLoc] = _UINT_1;
	common_byte_add(num, list);
}
void SEMI_E5::add_int4(byte num, byte *list) {
	_SendMsg[buffLoc] = _INT_4;
	common_byte_add_endian(list, num, 4);
}
void SEMI_E5::add_int8(byte num, byte *list) {
	_SendMsg[buffLoc] = _INT_8;
	common_byte_add_endian(list, num, 8);
}
void SEMI_E5::add_ft8(byte num, byte *list) {
	_SendMsg[buffLoc] = _FT_8;
	common_byte_add_endian(list, num, 8);
}

void SEMI_E5::add_binary(byte num, byte *list) {
	_SendMsg[buffLoc] = _BINARY;
	common_byte_add(num, list);
}
void SEMI_E5::common_byte_add(byte num, byte *list) {
	int i;
	_SendMsg[buffLoc + 1] = num;
	buffLoc += 2;
	for (i = 0; i < num; i++) {
		_SendMsg[buffLoc] = *(list + i);
		buffLoc++;
	}
}
void SEMI_E5::common_byte_add_endian(byte *list, byte num, byte typeSize) {
	int i, j;
	_SendMsg[buffLoc + 1] = num * typeSize;
	buffLoc += 2;
	for (i = 0; i < num; i++) {
		for (j = 0; j < typeSize; j++) {
			_SendMsg[buffLoc] = *(list + (i * typeSize + (typeSize - j - 1)));
			buffLoc++;
		}
	}
}

/*************************************************************************/
//  (decoder)units to output messages.
/*************************************************************************/
byte SEMI_E5::dList_msg(byte *inBlock) {
	if (inBlock[0] != 0x01)
		return _FORMAT_ERROR_;
	return (inBlock[1]);
}
byte SEMI_E5::d_ascii_msg(byte *dataBlock) {
	byte ilength;
	ilength = dataBlock[1];
	if (dataBlock[0] != 0x41)
		return _FORMAT_ERROR_;
	return (ilength);
}
byte SEMI_E5::d_uint2(unsigned int *num, byte *dataBlock) {
	if (dataBlock[0] != 0xA9 || dataBlock[1] != 2)
		return _FORMAT_ERROR_;
	num[0] = dataBlock[2] * 256 + dataBlock[3];
	return TRUE;
}
