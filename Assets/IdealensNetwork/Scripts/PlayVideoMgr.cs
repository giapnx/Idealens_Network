using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PlayVideoMgr : SingletonMonoBehaviour<PlayVideoMgr> {

	public VideoPlayer videoPlayer;
	public VideoClip[] videoGallery;

	public delegate void DoneLoadVideo();
	public static event DoneLoadVideo DoneLoadVideoEvent;

	// Use this for initialization
	void Start () {
		StartCoroutine (PreloadVideoTime (1f));

		CommandMgr CommandMgrInstance = CommandMgr.Instance;
		CommandMgrInstance.RegisterUsePressPlayEvent (HandlerUserPressPlay);
		CommandMgrInstance.RegisterUsePressPauseEvent (HandlerUserPressPause);
		CommandMgrInstance.RegisterUsePressStopEvent (HandlerUserPressStop);
		CommandMgrInstance.RegisterUsePressReplayEvent (HandlerUserPressReplay);
	}
	
//	 Update is called once per frame
//	void Update ()
//	{
//		
//	}

	public void SwitchVideo(int index)
	{
		videoPlayer.Stop ();
		videoPlayer.clip = videoGallery [index];
		StartCoroutine (PreloadVideoTime (1f));
	}

	IEnumerator PreloadVideoTime(float time)
	{
		videoPlayer.Play ();
		yield return new WaitForSeconds (time);
		videoPlayer.Stop ();

		if (DoneLoadVideoEvent != null) 
		{
			DoneLoadVideoEvent ();
		}
	}

	public void HandlerUserPressPlay()
	{
		videoPlayer.Play ();
	}

	public void HandlerUserPressPause()
	{
		videoPlayer.Pause ();
	}

	public void HandlerUserPressStop()
	{
		videoPlayer.Stop ();
	}

	public void HandlerUserPressReplay()
	{
		StartCoroutine (HandlerUserPressReplayCo());
	}

	IEnumerator HandlerUserPressReplayCo()
	{
		videoPlayer.Stop ();
		yield return new WaitForSeconds (1);
		videoPlayer.Play ();
	}

}
