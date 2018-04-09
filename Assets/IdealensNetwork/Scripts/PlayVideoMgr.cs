using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PlayVideoMgr : SingletonMonoBehaviour<PlayVideoMgr> {

	public VideoPlayer videoPlayer;

	// Use this for initialization
	void Start () {
		StartCoroutine (StopVideoCo ());

		CommandMgr CommandMgrInstance = CommandMgr.Instance;
		CommandMgrInstance.RegisterUsePressPlayEvent (HandlerUserPressPlay);
		CommandMgrInstance.RegisterUsePressPauseEvent (HandlerUserPressPause);
		CommandMgrInstance.RegisterUsePressStopEvent (HandlerUserPressStop);
		CommandMgrInstance.RegisterUsePressReplayEvent (HandlerUserPressReplay);
	}
	
//	 Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			if (videoPlayer.isPlaying) {
				videoPlayer.Pause ();
			} else {
				videoPlayer.Play ();
			}
		}

		if (Input.GetKeyDown (KeyCode.R)) 
		{
			videoPlayer.Stop ();
			videoPlayer.Play ();
		}
	}

	IEnumerator StopVideoCo()
	{
		yield return new WaitForSeconds (2f);
		videoPlayer.Stop ();
	}

	void HandlerUserPressPlay()
	{
		videoPlayer.Play ();
	}

	void HandlerUserPressPause()
	{
		videoPlayer.Pause ();
	}

	void HandlerUserPressStop()
	{
		videoPlayer.Stop ();
	}

	void HandlerUserPressReplay()
	{
		videoPlayer.Stop ();
		videoPlayer.Play ();
	}

}
