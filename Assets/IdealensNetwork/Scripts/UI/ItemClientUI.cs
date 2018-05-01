using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemClientUI : MonoBehaviour {

	public static ItemClientUI Instance;

	public int index;
	public Text indexText;
	public Image clientStatus;
	public Image videoStatus;
	public Text timeText;

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void InitItemClient(int _index)
	{
		index = _index;
		indexText.text = index.ToString ();
		SetClientStatus (ListClientUI.Instance.connectingColor);
		SetVideoStatus (ListClientUI.Instance.stopImage);
		SetTimeText (0);
	}

	public void SetClientStatus(Color _clientStatus)
	{
		clientStatus.color = _clientStatus;
	}

	public void SetVideoStatus(Sprite _videoStatus)
	{
		videoStatus.sprite = _videoStatus;
	}

	public void SetTimeText(int time)
	{
		if (time < 60) 
		{
			timeText.text = "t: " + time;
		}
		else
		{
			int m = time / 60;
			int s = time % 60;
			timeText.text = string.Format ("t: {0}:{1}", m, s);
		}
	}

}
