using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ServerVideoProgressMgr : SingletonMonoBehaviour<ServerVideoProgressMgr> 
{
	public Slider progressBarSlider;
	public Text timeText;
	public Text timeEndText;
	float lengthOfVideo;

	float _countTime;
	ServerVideoPlayer VideoPlayerInstance;

	// Use this for initialization
	void Start () 
	{
		VideoPlayerInstance = ServerVideoPlayer.Instance;
//		SetDefault ();
	}

	void OnEnable()
	{
		ServerVideoPlayer.PreloadCompleted += SetDefault;
	}

	void OnDisable()
	{
		ServerVideoPlayer.PreloadCompleted -= SetDefault;
	}

	void Update()
	{
		if (VideoPlayerInstance.videoPlayer.isPlaying) 
		{
			_countTime += Time.deltaTime;

			SetProgressBar ();
		}
	}
	
	public void SetDefault()
	{

		lengthOfVideo = (float)(VideoPlayerInstance.videoPlayer.frameCount / VideoPlayerInstance.videoPlayer.frameRate);

		timeText.text = "0:00";
		TimeSpan time = TimeSpan.FromSeconds (lengthOfVideo);
		if(time.Hours > 0)
			timeEndText.text = string.Format ("{0:D1}:{1:D1}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
		else
			timeEndText.text = string.Format ("{0:D1}:{1:D2}", time.Minutes, time.Seconds);

		progressBarSlider.maxValue = lengthOfVideo;
		progressBarSlider.value = 0;
	}

	public void SetProgressBar()
	{
		progressBarSlider.value = _countTime;
		TimeSpan time = TimeSpan.FromSeconds (_countTime);
		if(time.Hours > 0)
			timeText.text = string.Format ("{0:D1}:{1:D1}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
		else
			timeText.text = string.Format ("{0:D1}:{1:D2}", time.Minutes, time.Seconds);
	}

	public void SetCountTime(float time)
	{
		_countTime = time;
	}
}
