using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class VideoProgressMgr : SingletonMonoBehaviour<VideoProgressMgr> 
{
	public Slider progressBarSlider;
	public Text timeText;
	public Text timeEndText;
	float lengthOfVideo;

	float _countTime;
	PlayVideoMgr PlayVideoInstance;

	// Use this for initialization
	void Start () 
	{
		PlayVideoInstance = PlayVideoMgr.Instance;
		SetDefault ();
	}

	void Update()
	{
		if (PlayVideoInstance.videoPlayer.isPlaying) 
		{
			_countTime += Time.deltaTime;

			SetProgressBar ();
		}
	}
	
	public void SetDefault()
	{
		lengthOfVideo = (float)PlayVideoMgr.Instance.videoPlayer.clip.length;

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
