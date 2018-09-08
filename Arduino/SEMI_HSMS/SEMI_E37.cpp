/*
 SEMI_E37(HSMS)
 developer : Liu Yu Yuan
 Data : 20161115
 */
#include "SEMI_E37.h"

SEMI_E37::SEMI_E37(void) {
	this->_RecvLen = NULL;
	this->_RecvMsg = NULL;
	this->_SendLen = NULL;
	this->_SendMsg = NULL;

	this->signals = NULL;
	this->rowNum = 0;
	this->colNum = 0;
}
SEMI_E37::~SEMI_E37(void) {
	for(int i=0; i< this->rowNum;i++)
		delete this->signals[i];
	delete this->signals;
}
void SEMI_E37::INIT_SEMI_E37(byte *RecvMsg, int *RecvLen, byte *SendMsg,
		int *SendLen, double **signals, int32_t rowNum, int32_t colNum) {
	_RecvMsg = RecvMsg;
	_RecvLen = RecvLen;
	_SendMsg = SendMsg;
	_SendLen = SendLen;

	this->rowNum = rowNum;
	this->colNum = colNum;

	this->signals =signals;

	SEMIe5.INIT_SEMI_E5(RecvMsg, RecvLen, SendMsg, SendLen, this->signals,
			&this->rowNum, &this->colNum);
}

bool SEMI_E37::MSG_Analyze() {
	if (*_RecvLen >= 14) {
		long MgsLen = _RecvMsg[LEN_INDEX] * 0x10000
				+ _RecvMsg[LEN_INDEX + 1] * 0x100
				+ _RecvMsg[LEN_INDEX + 2] * 0x100 + _RecvMsg[LEN_INDEX + 3];
//    if(MgsLen == (*_RecvLen - MSG_FIX_LEN)){
		if ((MgsLen == MSG_HEARER_LEN) && (_RecvMsg[STYPE_INDEX] != 0)) {
			//Test message => S0F0
			for (int i = 0; i < *_RecvLen; i++) {
				_SendMsg[i] = _RecvMsg[i];
			}
			*_SendLen = *_RecvLen;
			if (_RecvMsg[STYPE_INDEX] == 1) {
				//CONNECT
				_SendMsg[STYPE_INDEX] = 2;
			} else if (_RecvMsg[STYPE_INDEX] == 5) {
				//LINKTEST
				_SendMsg[STYPE_INDEX] = 6;
			} else if (_RecvMsg[STYPE_INDEX] == 9) {
				//DISCONNECT
				return false; // do not reply
			}
		} else {
			//DATA
			if (!SEMIe5.SECS_COMMAD()) {
				// something wrong
				return false;
			}
		}
		//normal reply
		return true;
//    }
//    else{
//      // length byte is not equal (header length + messgae length)
//      // if need to reply??
//      return false; 
//    }
	} else {
		// Total Length less than 14 bytes
		// if need to reply??
		return false;
	}

}

bool SEMI_E37::GetWBit(void) {
	if ((*_RecvLen) < 10)
		return 0;
	return _RecvMsg[STREAM_INDEX] & 0x80;
}

byte SEMI_E37::GetStream(void) {
	if ((*_RecvLen) < 10)
		return 0;
	return _RecvMsg[STREAM_INDEX] & 0x7f;
}

byte SEMI_E37::GetFunction(void) {
	if ((*_RecvLen) < 10)
		return 0;
	return _RecvMsg[FUNCTION_INDEX];
}

int SEMI_E37::GetRcvLength(void) {
	return *_RecvLen;
}
byte SEMI_E37::GetSType(void) {
	if ((*_RecvLen) < 10)
		return 0;
	return _RecvMsg[STYPE_INDEX];
}

int32_t SEMI_E37::GetMsgLength(void) {
	if (*_RecvLen < 14)
		return -1;
	return _RecvMsg[LEN_INDEX] * 0x10000 + _RecvMsg[LEN_INDEX + 1] * 0x100
			+ _RecvMsg[LEN_INDEX + 2] * 0x100 + _RecvMsg[LEN_INDEX + 3];
}
