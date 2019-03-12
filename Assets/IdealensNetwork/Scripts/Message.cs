using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Message
{
	// Client -> Server
	public const byte CLIENT_ACTIVE 	= 0x40;
	public const byte CLIENT_INACTIVE 	= 0x41;
	public const byte CLIENT_DISCONNECT = 0x42;
	public const byte CLIENT_PLAY_VIDEO = 0x43;
	public const byte CLIENT_PAUSE_VIDEO 	= 0x44;
	public const byte CLIENT_STOP_VIDEO 	= 0x45;
	public const byte CLIENT_SEND_LIST_VIDEO 	= 0x46;
	public const byte CLIENT_DONE_RECEIVE_VIDEO = 0x47;
	public const byte CLIENT_PERCENT_RECEIVE = 0x48;

	// Server -> Client
	public const byte PLAY_VIDEO 	= 0x2F;
	public const byte PAUSE_VIDEO 	= 0x31;
	public const byte STOP_VIDEO 	= 0x32;
	public const byte REPLAY_VIDEO 	= 0x33;
	public const byte SWITCH_VIDEO_CLIP = 0x34;
	public const byte SWITCH_VIDEO_URL 	= 0x35;
	public const byte GET_LIST_VIDEO 	= 0x36;
	public const byte SEND_VIDEO 		= 0x37;

	public static byte[] Pack(byte msgType, string msgData)
	{
		byte[] msgDataByte = System.Text.Encoding.ASCII.GetBytes(msgData);
//		byte[] lengthOfMsg = BitConverter.GetBytes (msgDataByte.Length); // 4 bytes
		byte[] eofByte = System.Text.Encoding.ASCII.GetBytes ("<EOF>");
		byte[] msg = new byte[1 + msgDataByte.Length + eofByte.Length];

		// Msg format: msgType_msgData_<EOF>
		msg [0] = msgType;
//		lengthOfMsg.CopyTo (msg, 1);
		msgDataByte.CopyTo (msg, 1);
		eofByte.CopyTo (msg, 1 + msgDataByte.Length);
		Debug.Log ("Pack: " + System.Text.Encoding.ASCII.GetString (msg));

		return msg;
	}

	public static byte[] Pack(byte msgType)
	{
		byte[] eofByte = System.Text.Encoding.ASCII.GetBytes ("<EOF>");
		byte[] msg = new byte[1 + eofByte.Length];

		msg [0] = msgType;
		eofByte.CopyTo (msg, 1);
		Debug.Log ("Pack: " + System.Text.Encoding.ASCII.GetString (msg));

		return msg;
	}
}
