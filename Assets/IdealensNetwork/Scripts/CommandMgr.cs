using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class CommandMgr : SingletonMonoBehaviour<CommandMgr> {

	private static CommandMgr _instance;
	public static CommandMgr Instance {
		get {
			if (_instance == null) {
				_instance = new GameObject ("CommandMgr").AddComponent <CommandMgr> ();
			}

			return _instance;
		}
	}

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

	void Awake()
	{
		_instance = this;
	}

	// Update is called once per frame
	void Update ()
	{
//		if (receivedCMD != Command.NONE)
//		{
//			ExecuteCommand ();
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
			break;

		case Command.PAUSE:
			OnUserPressPauseHandler.Invoke ();
			break;

		case Command.STOP:
			OnUserPressStopHandler.Invoke ();
			break;

		case Command.REPLAY:
			OnUserPressReplayHandler.Invoke ();
			break;

		case Command.SWITCH_CLIP:
			if (OnUserPressSwitchVideoClipHandler != null) {
				OnUserPressSwitchVideoClipHandler (int.Parse (_msg [1]));
			}
			break;
		case Command.SWITCH_URL:
			if (OnUserPressSwitchVideoUrlHandler != null) {
				OnUserPressSwitchVideoUrlHandler (_msg [1]);
			}
			break;

		default: // None
			break;
		}

	}

//	public void CmdTellServerCmd(Command cmd)
//	{
//		receivedCMD = cmd;
//		print ("CMD");
//	}
}
