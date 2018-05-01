using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Timers;
using UnityEngine.Video;

public class ClientCommandMgr : SingletonMonoBehaviour<ClientCommandMgr> {

//	private static ClientCommandMgr _instance;
//	public static ClientCommandMgr Instance {
//		get {
//			if (_instance == null) {
//				_instance = new GameObject ("CommandMgr").AddComponent <ClientCommandMgr> ();
//			}
//
//			return _instance;
//		}
//	}

	#region Action - Register
	event Action OnUserPressPlayHandler;
	event Action OnUserPressPauseHandler;
	event Action OnUserPressStopHandler;
	event Action OnUserPressReplayHandler;
	event Action<int> OnUserPressSwitchVideoClipHandler;
	event Action<string> OnUserPressSwitchVideoUrlHandler;

	public void RegisterUsePressPlayEvent(Action callback) 	{ OnUserPressPlayHandler 	+= callback;}
	public void RegisterUsePressPauseEvent(Action callback) { OnUserPressPauseHandler 	+= callback;}
	public void RegisterUsePressStopEvent(Action callback) 	{ OnUserPressStopHandler 	+= callback;}
	public void RegisterUsePressReplayEvent(Action callback) { OnUserPressReplayHandler += callback;}
	public void RegisterUsePressSwitchVideoClipEvent(Action<int> callback) { OnUserPressSwitchVideoClipHandler += callback;}
	public void RegisterUsePressSwitchVideoUrlEvent(Action<string> callback) { OnUserPressSwitchVideoUrlHandler += callback;}
	#endregion

	Command receivedCMD = Command.NONE;
	public enum Command
	{
		NONE,
		PLAY,
		PAUSE,
		STOP,
		REPLAY,
		SWITCH_CLIP,
		SWITCH_URL
	}

//	float timeVideoCount = 0;
//	Timer aTimer = new Timer(1000);
	VideoPlayer videoPlayer;

//	void Awake()
//	{
//		_instance = this;
//	}
//
	void Start()
	{
//		aTimer.Elapsed += new ElapsedEventHandler (OnTimedEvent);
		videoPlayer = ClientVideoPlayer.Instance.videoPlayer;
		videoPlayer.loopPointReached += HandlerEndVideo;
	}

	void OnDisable()
	{
		videoPlayer.loopPointReached -= HandlerEndVideo;
	}

	// Update is called once per frame
	void Update ()
	{
//		if (videoPlayer.isPlaying)
//		{
//			timeVideoCount += Time.deltaTime;
//			aTimer.Enabled = true;
//		}
//		else
//		{
//			aTimer.Enabled = false;
//		}
	}

	public void ExecuteCommand(object msg)
	{
		print ("CMD: " + msg);
		string[] _msg = msg.ToString ().Split ("|" [0]);

		receivedCMD = (Command)Enum.Parse (typeof(Command), _msg[0]);

		switch (receivedCMD)
		{
		case Command.PLAY:
			if (OnUserPressPlayHandler != null) { OnUserPressPlayHandler.Invoke ();	}
			TCPClient.Instance.SendMessage (MessageType.VIDEO_PLAY.ToString ());
			break;

		case Command.PAUSE:
			OnUserPressPauseHandler.Invoke ();
			TCPClient.Instance.SendMessage (MessageType.VIDEO_PAUSE.ToString ());
			break;

		case Command.STOP:
			OnUserPressStopHandler.Invoke ();
			TCPClient.Instance.SendMessage (MessageType.VIDEO_STOP.ToString ());
//			timeVideoCount = 0;
			break;

		case Command.REPLAY:
			OnUserPressReplayHandler.Invoke ();
			TCPClient.Instance.SendMessage (MessageType.VIDEO_PLAY.ToString ());
//			timeVideoCount = 0;
			break;

		case Command.SWITCH_CLIP:
			if (OnUserPressSwitchVideoClipHandler != null) {
				OnUserPressSwitchVideoClipHandler (int.Parse (_msg [1]));
			}
			TCPClient.Instance.SendMessage (MessageType.VIDEO_STOP.ToString ());
//			timeVideoCount = 0;
			break;
		case Command.SWITCH_URL:
			if (OnUserPressSwitchVideoUrlHandler != null) {
				OnUserPressSwitchVideoUrlHandler (_msg [1]);
			}
			TCPClient.Instance.SendMessage (MessageType.VIDEO_STOP.ToString ());
//			timeVideoCount = 0;
			break;

		default: // None
			break;
		}
	}

	void HandlerEndVideo(VideoPlayer _vp)
	{
		TCPClient.Instance.SendMessage (MessageType.VIDEO_STOP.ToString ());
	}

	void OnApplicationPause(bool pauseStatus)
	{
//		print ("video playing? - " + videoPlayer.isPlaying);
//		if (pauseStatus)
//		{
//			SendMessage (MessageType.VIDEO_PAUSE.ToString ());
//		}
//		else
//		{
//			if (videoPlayer != null && videoPlayer.isPlaying)
//			{
//				SendMessage (MessageType.VIDEO_PLAY.ToString ());
//			}
//		}
	}

//	void OnTimedEvent(object source, ElapsedEventArgs e)
//	{
//		print ("time: " + Mathf.RoundToInt(timeVideoCount));
//		string msg = MessageType.VIDEO_PLAY + "|" + Mathf.RoundToInt (timeVideoCount);
//		TCPClient.Instance.SendMessage (msg);
//	}

//	public void CmdTellServerCmd(Command cmd)
//	{
//		receivedCMD = cmd;
//		print ("CMD");
//	}
}
