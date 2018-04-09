﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CMDControllerMgr : MonoBehaviour {

	#region UI
	public Button PlayBtn;
	public Text playBtnText;
	string _playBtnString;

	//	public Button StopBtn;
	//	public Text textStopBtn;

	//	public Button ReplayBtn;
	//	public Text textReplayBtn;

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

	public const string PLAYING_TEXT = "PLAYING";
	public const string PAUSED_TEXT = "PAUSED";
	public const string STOPED_TEXT = "STOPED";
	#endregion

	bool isPlaying;

	CommandMgr CommandMgrInstante;
	CommandMgr.Command _cmd;

	// Use this for initialization
	void Start () {
		CommandMgrInstante = CommandMgr.Instance;
		SetDefault ();
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

	public void OnClickConfirm()
	{
		SetStatusText (_statusString);
		playBtnText.text = _playBtnString;
		StartCoroutine (CmdTellServerCo ());
	}

	IEnumerator CmdTellServerCo()
	{
		CommandMgrInstante.CmdTellServerCmd (_cmd);
		yield return new WaitForEndOfFrame();
		CommandMgrInstante.CmdTellServerCmd (CommandMgr.Command.NONE);
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
