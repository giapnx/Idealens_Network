using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class ServerVideoPlayer : SingletonMonoBehaviour<ServerVideoPlayer>
{

  public VideoClip[] videoGallery;
  [HideInInspector]
  public VideoPlayer videoPlayer;
  public VideoState videoState = VideoState.STOPPED;
  enum VideoState
  {
    PLAYING,
    PAUSED,
    STOPPED
  }

  public VideoLocate currentVideoLocate = VideoLocate.INTERNAL;
	public int videoClipIndex = -1;
  public string videoUrlName;


  private AudioSource audioSource;
  private bool isLoadCompleted = false;

  public delegate void DoneLoadVideo();
  public static event DoneLoadVideo PreloadCompleted;

  // Use this for initialization
  void Start()
  {
    videoPlayer = GetComponent<VideoPlayer>();
    audioSource = GetComponent<AudioSource>();

    SetDefault();
    OnClickSwitchVideoClip(0);
    //		HandlerUserPressSwitchVideoUrl ("Url_Video1.mp4");
  }

  void OnDisable()
  {
    videoPlayer.prepareCompleted -= PreloadVideoTime;
    videoPlayer.loopPointReached -= EndReached;
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
    videoPlayer.loopPointReached += EndReached;
    videoPlayer.playOnAwake = false;

    //Set Audio Output to AudioSource
    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

    // Assign the Audio from Video to AudioSource to be played
    videoPlayer.EnableAudioTrack(0, true);
    videoPlayer.SetTargetAudioSource(0, audioSource);
  }

  void EndReached(VideoPlayer vp)
  {
    videoState = VideoState.STOPPED;
    print("Video stopped");
  }

  public void OnClickSwitchVideoClip(int index)
  {
    if (videoPlayer.isPlaying)
    {
      videoPlayer.Stop();
      videoState = VideoState.STOPPED;
    }

    videoPlayer.source = VideoSource.VideoClip;
    videoPlayer.clip = videoGallery[index];

    StopAllCoroutines();
    isLoadCompleted = false;

    videoPlayer.Prepare();
    StartCoroutine(CheckPrepareVideo());

		currentVideoLocate = VideoLocate.INTERNAL;
		videoClipIndex = index;
  }

  public void OnClickSwitchVideoUrl(string videoName)
  {
    string _url = Application.persistentDataPath + "/Video/" + videoName;
    if (!File.Exists(_url))
    {
      print("Video not exist");
      return;
    }
    if (videoPlayer.isPlaying)
    {
      videoPlayer.Stop();
      videoState = VideoState.STOPPED;
    }

    videoPlayer.source = VideoSource.Url;
    videoPlayer.url = _url;

    StopAllCoroutines();
    isLoadCompleted = false;
    print("Preparing !");

    videoPlayer.Prepare();
    StartCoroutine(CheckPrepareVideo());

		currentVideoLocate = VideoLocate.EXTERNAL;
		videoUrlName = videoName;
  }

  void PreloadVideoTime(VideoPlayer _vp)
  {
    if (!isLoadCompleted)
      StartCoroutine(PreloadVideoTimeSwitch(_vp));
  }

  IEnumerator PreloadVideoTimeSwitch(VideoPlayer _vp)
  {
    _vp.Play();
    yield return new WaitForSeconds(1);
    _vp.Stop();
    isLoadCompleted = true;
    print("Loaded!");
  }

  IEnumerator CheckPrepareVideo()
  {
    yield return new WaitForSeconds(0.3f);
    print("prepare continue");
    if (!videoPlayer.isPrepared)
    {
      videoPlayer.Prepare();
    }
  }

  public void OnClickPlay()
  {
    videoPlayer.Play();
    videoState = VideoState.PLAYING;
  }

  public void OnClickPause()
  {
    videoPlayer.Pause();
    videoState = VideoState.PAUSED;
  }

  public void OnClickStop()
  {
    videoPlayer.Stop();
    videoState = VideoState.STOPPED;
  }

  public void OnClickReplay()
  {
    StartCoroutine(OnClickReplayCo());
  }

  IEnumerator OnClickReplayCo()
  {
    videoPlayer.Stop();
    yield return new WaitForSeconds(1);
    videoPlayer.Play();
    videoState = VideoState.PLAYING;
  }

}

  public enum VideoLocate
  {
    INTERNAL,  // play video located by Internal data
    EXTERNAL	// // play video located by External data (./Video/..)
  }
