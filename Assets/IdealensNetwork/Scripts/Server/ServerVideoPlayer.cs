using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class ServerVideoPlayer : SingletonMonoBehaviour<ServerVideoPlayer>
{

    public VideoClip[] videoGallery;
    [HideInInspector]
    public VideoPlayer videoPlayer;
    public VideoState currentVideoState = VideoState.STOPPED;

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
        currentVideoState = VideoState.STOPPED;
    }

    public void OnClickSwitchVideoClip(int index)
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            currentVideoState = VideoState.STOPPED;
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
            currentVideoState = VideoState.STOPPED;
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

        PreloadCompleted();
    }

    IEnumerator PreloadVideoTimeSwitch(VideoPlayer _vp)
    {
        _vp.Play();
        yield return new WaitForSeconds(1);
        _vp.Stop();
        currentVideoState = VideoState.STOPPED;
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
        currentVideoState = VideoState.PLAYING;
    }

    public void OnClickPause()
    {
        videoPlayer.Pause();
        currentVideoState = VideoState.PAUSED;
    }

    public void OnClickStop()
    {
        videoPlayer.Stop();
        currentVideoState = VideoState.STOPPED;
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
        currentVideoState = VideoState.PLAYING;
    }

}

public enum VideoLocate
{
    INTERNAL,  // play video located by Internal data
    EXTERNAL    // // play video located by External data (./Video/..)
}

public enum VideoState
{
    PLAYING,
    PAUSED,
    STOPPED
}
