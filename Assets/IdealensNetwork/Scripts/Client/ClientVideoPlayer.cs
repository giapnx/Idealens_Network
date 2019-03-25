using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class ClientVideoPlayer : SingletonMonoBehaviour<ClientVideoPlayer>
{

    public VideoClip[] videoGallery;
    [HideInInspector]
    public VideoPlayer videoPlayer;

    private AudioSource audioSource;
    private bool isLoadCompleted = false;

    public delegate void DoneLoadVideo();
    public static event DoneLoadVideo PreloadCompleted;

    public VideoState currentVideoState = VideoState.STOPPED;
    public VideoLocate currentVideoLocate = VideoLocate.INTERNAL;
    public int videoClipIndex = -1;
    public string videoUrlName;

    // Use this for initialization
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        audioSource = GetComponent<AudioSource>();

        SetDefault();
        HandlerUserPressSwitchVideoClip(0);
        //		HandlerUserPressSwitchVideoUrl ("Url_Video1.mp4");

        ClientCommandMgr CommandMgrInstance = ClientCommandMgr.Instance;
        CommandMgrInstance.RegisterUsePressPlayEvent(HandlerUserPressPlay);
        CommandMgrInstance.RegisterUsePressPauseEvent(HandlerUserPressPause);
        CommandMgrInstance.RegisterUsePressStopEvent(HandlerUserPressStop);
        CommandMgrInstance.RegisterUsePressReplayEvent(HandlerUserPressReplay);
        CommandMgrInstance.RegisterUsePressSwitchVideoClipEvent(HandlerUserPressSwitchVideoClip);
        CommandMgrInstance.RegisterUsePressSwitchVideoUrlEvent(HandlerUserPressSwitchVideoUrl);
        CommandMgrInstance.RegisterUsePressSeekEvent(HandlerUserPressSeek);

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

    public void HandlerUserPressSwitchVideoClip(int index)
    {
        if (currentVideoLocate != VideoLocate.INTERNAL || videoClipIndex != index)
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

            currentVideoLocate = VideoLocate.INTERNAL;
            videoClipIndex = index;
        }
    }

    public void HandlerUserPressSwitchVideoUrl(string videoName)
    {
        if (currentVideoLocate != VideoLocate.EXTERNAL || videoUrlName != videoName)
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

            currentVideoLocate = VideoLocate.EXTERNAL;
            videoUrlName = videoName;
        }
    }

    void PreloadVideoTime(VideoPlayer _vp)
    {
        if (!isLoadCompleted)
            StartCoroutine(PreloadVideoTimeSwitch(_vp));
    }

    IEnumerator PreloadVideoTimeSwitch(VideoPlayer _vp)
    {
        print("Preload");
        _vp.Play();
        yield return new WaitForSeconds(1);
        _vp.Stop();
        currentVideoState = VideoState.STOPPED;
        isLoadCompleted = true;
        print("Loaded!");
    }

    public void HandlerUserPressPlay()
    {
        videoPlayer.Play();
        currentVideoState = VideoState.PLAYING;
    }

    public void HandlerUserPressPause()
    {
        videoPlayer.Pause();
        currentVideoState = VideoState.PAUSED;
    }

    public void HandlerUserPressStop()
    {
        videoPlayer.Stop();
        currentVideoState = VideoState.STOPPED;
    }

    public void HandlerUserPressReplay()
    {
        StartCoroutine(HandlerUserPressReplayCo());
    }

    IEnumerator HandlerUserPressReplayCo()
    {
        videoPlayer.Stop();
        yield return new WaitForSeconds(1);
        videoPlayer.Play();
        currentVideoState = VideoState.PLAYING;
    }

    public void HandlerUserPressSeek(double nTime)
    {
        if (!videoPlayer.isPrepared)
        {
            return;
        }
        videoPlayer.time = nTime;
        print("Seek to: " + nTime);
    }
}
