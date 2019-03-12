using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class ServerVideoPlayer : SingletonMonoBehaviour<ServerVideoPlayer> {

	public VideoClip[] videoGallery;
	[HideInInspector]
	public VideoPlayer videoPlayer;

	private AudioSource audioSource;
	private bool isLoadCompleted = false;

	public delegate void DoneLoadVideo();
	public static event DoneLoadVideo PreloadCompleted;

	// Use this for initialization
	void Start ()
	{
		videoPlayer = GetComponent <VideoPlayer> ();
		audioSource = GetComponent <AudioSource> ();

		SetDefault ();
		OnClickSwitchVideoClip (0);
//		HandlerUserPressSwitchVideoUrl ("Url_Video1.mp4");
	}

	void OnDisable()
	{
		videoPlayer.prepareCompleted -= PreloadVideoTime;
	}
	
//	 Update is called once per frame
//	void Update ()
//	{
//		
//	}

	/// <summary>
	/// Set default of value for VideoPlayer.
	/// </summary>
	void SetDefault()
	{
		videoPlayer.prepareCompleted += PreloadVideoTime;
		videoPlayer.playOnAwake = false;

		//Set Audio Output to AudioSource
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

		// Assign the Audio from Video to AudioSource to be played
		videoPlayer.EnableAudioTrack (0, true);
		videoPlayer.SetTargetAudioSource (0, audioSource);
	}

	public void OnClickSwitchVideoClip(int index)
	{
		if (videoPlayer.isPlaying) { videoPlayer.Stop ();}

		videoPlayer.source = VideoSource.VideoClip;
		videoPlayer.clip = videoGallery [index];

//		//Set Audio Output to AudioSource
//		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
//
//		// Assign the Audio from Video to AudioSource to be played
//		videoPlayer.EnableAudioTrack (0, true);
//		videoPlayer.SetTargetAudioSource (0, audioSource);

		StopAllCoroutines ();
		isLoadCompleted = false;

		videoPlayer.Prepare ();
		StartCoroutine (CheckPrepareVideo());
	}

	public void OnClickSwitchVideoUrl(string videoName)
	{
		string _url = Application.persistentDataPath + "/Video/" + videoName;
		if (!File.Exists (_url))
		{
			print ("Video not exist");
			return;
		}
		if (videoPlayer.isPlaying) { videoPlayer.Stop ();}

		videoPlayer.source = VideoSource.Url;
		videoPlayer.url = _url;

//		//Set Audio Output to AudioSource
//		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
//
//		// Assign the Audio from Video to AudioSource to be played
//		videoPlayer.EnableAudioTrack (0, true);
//		videoPlayer.SetTargetAudioSource (0, audioSource);

		StopAllCoroutines ();
		isLoadCompleted = false;
		print ("Preparing !");

		videoPlayer.Prepare ();
		StartCoroutine (CheckPrepareVideo());
	}

	void PreloadVideoTime(VideoPlayer _vp)
	{
		if (!isLoadCompleted)
			StartCoroutine (PreloadVideoTimeSwitch (_vp));
	}

	IEnumerator PreloadVideoTimeSwitch(VideoPlayer _vp)
	{
		_vp.Play ();
		yield return new WaitForSeconds (1);
		_vp.Stop ();
//		_vp.Prepare ();

//		if (PreloadCompleted != null) 
//		{
//			PreloadCompleted ();
//		}
		isLoadCompleted = true;
		print ("Loaded!");
//		print ("Time1: " + _vp.frameCount);
//		print ("Time2: " + _vp.frameRate);
	}

	IEnumerator CheckPrepareVideo()
	{
		yield return new WaitForSeconds (0.3f);
		print ("prepare continue");
		if (!videoPlayer.isPrepared) {
			videoPlayer.Prepare ();
		}
	}

	public void OnClickPlay()
	{
		videoPlayer.Play ();
	}

	public void OnClickPause()
	{
		videoPlayer.Pause ();
	}

	public void OnClickStop()
	{
		videoPlayer.Stop ();
//		videoPlayer.Prepare ();
	}

	public void OnClickReplay()
	{
		StartCoroutine (OnClickReplayCo());
	}

	IEnumerator OnClickReplayCo()
	{
		videoPlayer.Stop ();
//		videoPlayer.Prepare ();
		yield return new WaitForSeconds (1);
		videoPlayer.Play ();
	}

}
