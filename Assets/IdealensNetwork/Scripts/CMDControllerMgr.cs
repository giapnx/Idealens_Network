﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CMDControllerMgr : MonoBehaviour {

	#region UI
	public Button PlayBtn;
	public Text playBtnText;
	string _playBtnString;

	public Text StatusText;
	string _statusString;

	public GameObject ConfirmPopup;
	public Text confirmText;
	//	public Button ConfirmBtn;
	#endregion

	#region STRING
	public const string CONFIRM_PLAY_TEXT 	= "DO YOU LIKE TO PLAY VIDEO?";
	public const string CONFIRM_PAUSE_TEXT 	= "DO YOU LIKE TO PAUSE VIDEO?";
	public const string CONFIRM_STOP_TEXT	= "DO YOU LIKE TO STOP VIDEO?";
	public const string CONFIRM_REPLAY_TEXT = "DO YOU LIKE TO REPLAY VIDEO?";
	public const string CONFIRM_SWITCH_TEXT = "DO YOU LIKE TO SWITCH VIDEO?";

	public const string PLAYING_TEXT = "PLAYING";
	public const string PAUSED_TEXT = "PAUSED";
	public const string STOPED_TEXT = "STOPED";
	public const string SWITCH_TEXT = "SWITCH";
	#endregion

	bool isPlaying;
	string _msg;
	int _videoIndex = 0;

	CommandMgr CommandMgrInstante;
	CommandMgr.Command _cmd;

	// Use this for initialization
	void Start () 
	{
		CommandMgrInstante = CommandMgr.Instance;
		SetDefault ();
		ResetProgressBar ();
	}

	void OnEnable()
	{
		PlayVideoMgr.DoneLoadVideoEvent += SetDefault;
		PlayVideoMgr.DoneLoadVideoEvent += ResetProgressBar;
	}

	void OnDisable()
	{
		PlayVideoMgr.DoneLoadVideoEvent -= SetDefault;
		PlayVideoMgr.DoneLoadVideoEvent -= ResetProgressBar;
	}

	// Update is called once per frame
	void Update () {

	}

	public void SetDefault()
	{
		isPlaying = false;
		playBtnText.text = "PLAY";
		SetStatusText ("---");
	}

	void ResetProgressBar()
	{
		VideoProgressMgr.Instance.SetCountTime (0);
		VideoProgressMgr.Instance.SetProgressBar ();
	}

	public void OnClickConfirm()
	{
		SetStatusText (_statusString);
		playBtnText.text = _playBtnString;
		_msg = _cmd.ToString () + "|" + _videoIndex.ToString ();
		TCPServer.Instance.SendMessage (_msg);
		print (_cmd.ToString ());
		switch (_cmd) 
		{
		case CommandMgr.Command.PLAY:
			PlayVideoMgr.Instance.HandlerUserPressPlay ();
			break;

		case CommandMgr.Command.PAUSE:
			PlayVideoMgr.Instance.HandlerUserPressPause ();
			break;

		case CommandMgr.Command.STOP:
			PlayVideoMgr.Instance.HandlerUserPressStop ();
			ResetProgressBar ();
			break;

		case CommandMgr.Command.REPLAY:
			PlayVideoMgr.Instance.HandlerUserPressReplay ();
			ResetProgressBar ();
			break;

		case CommandMgr.Command.SWITCH:
			PlayVideoMgr.Instance.HandlerUserPressSwitchVideo (_videoIndex);
			break;
			
		default:
			break;
		}
	}

	public void OnClickPlay()
	{
		if (!isPlaying)
		{
			// Play
			SetText (CONFIRM_PLAY_TEXT, PLAYING_TEXT);
			_playBtnString = "PAUSE";
			isPlaying = true;
			_cmd = CommandMgr.Command.PLAY;
		}
		else // playing
		{
			// Pause
			SetText (CONFIRM_PAUSE_TEXT, PAUSED_TEXT);
			_playBtnString = "PLAY";
			isPlaying = false;
			_cmd = CommandMgr.Command.PAUSE;
		}
	}

	public void OnClickStop()
	{
		SetText (CONFIRM_STOP_TEXT, STOPED_TEXT);
		_playBtnString = "PLAY";
		isPlaying = false;
		_cmd = CommandMgr.Command.STOP;
	}

	public void OnClickReplay()
	{
		SetText (CONFIRM_REPLAY_TEXT, PLAYING_TEXT);
		_playBtnString = "PAUSE";
		isPlaying = true;
		_cmd = CommandMgr.Command.REPLAY;
	}

	public void OnClickSwithVideo(int index)
	{
		SetText (CONFIRM_SWITCH_TEXT, SWITCH_TEXT);
		_playBtnString = "PLAY";
		isPlaying = false;
		_cmd = CommandMgr.Command.SWITCH;
		_videoIndex = index;
	}

	void SetText(string _confirmText, string _status)
	{
		SetConfirmText (_confirmText);
		_statusString = _status;
		ConfirmPopup.SetActive (true);
	}

	void SetStatusText(string str)
	{
		StatusText.text = str;
	}

	void SetConfirmText(string str)
	{
		confirmText.text = str;
	}
}
