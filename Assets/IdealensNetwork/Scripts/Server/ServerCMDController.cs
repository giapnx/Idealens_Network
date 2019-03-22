using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ServerCMDController : SingletonMonoBehaviour<ServerCMDController> {

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
	byte[] _msg;
	int _videoIndex = 0;
	String _nameVideo;

//	CommandMgr CommandMgrInstante;
	ClientCommandMgr.Command _cmd;
	byte msgType;

	// Use this for initialization
	void Start () 
	{
//		CommandMgrInstante = CommandMgr.Instance;
		SetDefault ();
		ResetProgressBar ();
	}

	void OnEnable()
	{
		ServerVideoPlayer.PreloadCompleted += SetDefault;
		ServerVideoPlayer.PreloadCompleted += ResetProgressBar;
	}

	void OnDisable()
	{
		ServerVideoPlayer.PreloadCompleted -= SetDefault;
		ServerVideoPlayer.PreloadCompleted -= ResetProgressBar;
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
		ServerVideoProgressMgr.Instance.SetCountTime (0);
		ServerVideoProgressMgr.Instance.SetProgressBar ();
	}

	public void OnClickConfirm()
	{
		SetStatusText (_statusString);
		playBtnText.text = _playBtnString;

		AsynchronousSocketListener.Instance.SendAll (_msg);
		print (System.Text.Encoding.ASCII.GetString (_msg));
		switch (_cmd)
		{
		case ClientCommandMgr.Command.PLAY:
			ServerVideoPlayer.Instance.OnClickPlay ();
			break;

		case ClientCommandMgr.Command.PAUSE:
			ServerVideoPlayer.Instance.OnClickPause ();
			break;

		case ClientCommandMgr.Command.STOP:
			ServerVideoPlayer.Instance.OnClickStop ();
			ResetProgressBar ();
			break;

		case ClientCommandMgr.Command.REPLAY:
			ServerVideoPlayer.Instance.OnClickReplay ();
			ResetProgressBar ();
			break;

		case ClientCommandMgr.Command.SWITCH_CLIP:
			ServerVideoPlayer.Instance.OnClickSwitchVideoClip (_videoIndex);
			break;

		case ClientCommandMgr.Command.SWITCH_URL:
			ServerVideoPlayer.Instance.OnClickSwitchVideoUrl (_nameVideo);
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
			_cmd = ClientCommandMgr.Command.PLAY;
			msgType = Message.PLAY_VIDEO;
		}
		else // playing
		{
			// Pause
			SetText (CONFIRM_PAUSE_TEXT, PAUSED_TEXT);
			_playBtnString = "PLAY";
			isPlaying = false;
			_cmd = ClientCommandMgr.Command.PAUSE;
			msgType = Message.PAUSE_VIDEO;
		}
//		_msg = _cmd.ToString ();
		_msg = Message.Pack (msgType);
	}

	public void OnClickStop()
	{
		SetText (CONFIRM_STOP_TEXT, STOPED_TEXT);
		_playBtnString = "PLAY";
		isPlaying = false;
		_cmd = ClientCommandMgr.Command.STOP;
//		_msg = _cmd.ToString ();
		_msg = Message.Pack (Message.STOP_VIDEO);
	}

	public void OnClickReplay()
	{
		SetText (CONFIRM_REPLAY_TEXT, PLAYING_TEXT);
		_playBtnString = "PAUSE";
		isPlaying = true;
		_cmd = ClientCommandMgr.Command.REPLAY;
//		_msg = _cmd.ToString ();
		_msg = Message.Pack (Message.REPLAY_VIDEO);
	}

	public void OnClickSwithVideoClip(int index)
	{
		SetText (CONFIRM_SWITCH_TEXT, SWITCH_TEXT);
		_playBtnString = "PLAY";
		isPlaying = false;
		_cmd = ClientCommandMgr.Command.SWITCH_CLIP;
		_videoIndex = index;
//		_msg = _cmd.ToString () + "|" + _videoIndex.ToString ();
		_msg = Message.Pack (Message.SWITCH_VIDEO_CLIP, index.ToString ());
	}

	public void OnClickSwithVideoUrl(string nameVideo)
	{
		SetText (CONFIRM_SWITCH_TEXT, SWITCH_TEXT);
		_playBtnString = "PLAY";
		isPlaying = false;
		_cmd = ClientCommandMgr.Command.SWITCH_URL;
		_nameVideo = nameVideo;
//		_msg = _cmd.ToString () + "|" + _nameVideo;
		_msg = Message.Pack (Message.SWITCH_VIDEO_URL, nameVideo);
	}

    /// <summary>
    ///Test: Send cmd disconnect to First client
    /// </summary>
    public void OnClickDisconnectFirst()
    {
        _cmd = ClientCommandMgr.Command.DISCONNECT;
        _msg = Message.Pack(Message.DISCONNECT);

        AsynchronousSocketListener.Instance.SendToFirst(_msg);
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
