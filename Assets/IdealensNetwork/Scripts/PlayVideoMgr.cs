using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class PlayVideoMgr : SingletonMonoBehaviour<PlayVideoMgr> {

	public VideoPlayer videoPlayer;
	public VideoClip[] videoGallery;

	public delegate void DoneLoadVideo();
	public static event DoneLoadVideo DoneLoadVideoEvent;

    public int videoPlayingIndex;

    // Use this for initialization
    void Start () {
		StartCoroutine (PreloadVideoTime (1f));

		CommandMgr CommandMgrInstance = CommandMgr.Instance;
		CommandMgrInstance.RegisterUsePressPlayEvent (HandlerUserPressPlay);
		CommandMgrInstance.RegisterUsePressPauseEvent (HandlerUserPressPause);
		CommandMgrInstance.RegisterUsePressStopEvent (HandlerUserPressStop);
		CommandMgrInstance.RegisterUsePressReplayEvent (HandlerUserPressReplay);
		CommandMgrInstance.RegisterUsePressSwitchVideoEvent (HandlerUserPressSwitchVideo);
	}

    public void HandlerUserPressSwitchVideo(int index)
	{
		videoPlayer.Stop ();
		videoPlayer.clip = videoGallery [index];
        videoPlayingIndex = index;
        StartCoroutine (PreloadVideoTimeSwitch ());
	}

	IEnumerator PreloadVideoTimeSwitch()
	{
		yield return new WaitForSeconds (1);
		videoPlayer.Play ();
		yield return new WaitForSeconds (1);
		videoPlayer.Stop ();

		if (DoneLoadVideoEvent != null) 
		{
			DoneLoadVideoEvent ();
		}
	}

	IEnumerator PreloadVideoTime(float time)
	{
//		videoPlayer.Play ();
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
