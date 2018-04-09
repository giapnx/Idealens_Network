using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class CommandMgr : NetworkBehaviour {

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

	public void RegisterUsePressPlayEvent(Action callback) { OnUserPressPlayHandler 	+= callback;}
	public void RegisterUsePressPauseEvent(Action callback) { OnUserPressPauseHandler 	+= callback;}
	public void RegisterUsePressStopEvent(Action callback) { OnUserPressStopHandler 	+= callback;}
	public void RegisterUsePressReplayEvent(Action callback) { OnUserPressReplayHandler += callback;}
	#endregion

	public UnityEngine.UI.Text CmdText;

	[SyncVar]
	Command syncCmd = Command.NONE;
	public enum Command 
	{
		NONE,
		PLAY,
		PAUSE,
		STOP,
		REPLAY
	}

	void Awake()
	{
		_instance = this;
	}
	
	// Update is called once per frame
	void Update () 
	{
//		print ("Test -----");
		if (isServer) return;

		if (syncCmd != Command.NONE)
		{
			ExecuteCommand ();
		}
	}

	[Client]
	void ExecuteCommand()
	{
		print ("CMD: " + syncCmd.ToString ());
		CmdText.text = syncCmd.ToString ();
		switch (syncCmd)
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
			OnUserPressStopHandler.Invoke ();
			break;

		default: // None
			break;
		}

	}

	[Command]
	public void CmdTellServerCmd(Command cmd)
	{
		syncCmd = cmd;
		print ("CMD");
	}
}
