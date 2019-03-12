using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerUIController : SingletonMonoBehaviour<ServerUIController> 
{
	public Text ipText;
	public Text connectNumText;
	public Dropdown listVideoDropdown;
	public GameObject SendingPanel;
//	int selectedValue = -1;

	public Dictionary<string, ItemClientUI> clients = new Dictionary<string, ItemClientUI>();
	private int countConnectingClients = 0;
	private Sprite currentVideoStatus;

	void Start()
	{
		currentVideoStatus = ListClientUI.Instance.stopImage;
	}

	// Use this for initialization
	void OnEnable ()
	{
		AsynchronousSocketListener.OnServerStartListening 	+= SetIpText;

		AsynchronousSocketListener.OnClientConnectEvent 	+= OnClientConnect;
		AsynchronousSocketListener.OnClientDisconnectEvent 	+= OnClientDisconnect;
		AsynchronousSocketListener.OnClientActiveEvent 		+= OnClientActive;
		AsynchronousSocketListener.OnClientInactiveEvent 	+= OnClientInactive;
		AsynchronousSocketListener.OnClientPauseVideoEvent 	+= OnClientPauseVideo;
		AsynchronousSocketListener.OnClientStopVideoEvent 	+= OnClientStopVideo;
		AsynchronousSocketListener.OnClientPlayVideoEvent 	+= OnClientPlayVideo;
		AsynchronousSocketListener.OnRecieveListVideo 		+= OnReceiveListVideo;

		AsynchronousSocketListener.OnStartSendVideo += StartSendVideo;

	}

	void OnDisable ()
	{
		AsynchronousSocketListener.OnServerStartListening 	-= SetIpText;

		AsynchronousSocketListener.OnClientConnectEvent 	-= OnClientConnect;
		AsynchronousSocketListener.OnClientDisconnectEvent 	-= OnClientDisconnect;
		AsynchronousSocketListener.OnClientActiveEvent 		-= OnClientActive;
		AsynchronousSocketListener.OnClientInactiveEvent 	-= OnClientInactive;
		AsynchronousSocketListener.OnClientPauseVideoEvent 	-= OnClientPauseVideo;
		AsynchronousSocketListener.OnClientStopVideoEvent 	-= OnClientStopVideo;
		AsynchronousSocketListener.OnClientPlayVideoEvent 	-= OnClientPlayVideo;
		AsynchronousSocketListener.OnRecieveListVideo 		-= OnReceiveListVideo;

		AsynchronousSocketListener.OnStartSendVideo -= StartSendVideo;

	}
	
	// Update is called once per frame
//	void Update () 
//	{
//		
//	}



	void OnClientConnect(string ip)
	{
		if (!clients.ContainsKey (ip))
		{
			// create item client UI and update UI
			ItemClientUI go = ListClientUI.Instance.CreateNewClient (clients.Count + 1);
			clients.Add (ip, go);
		}
		else
		{
			OnClientActive (ip);
		}

		countConnectingClients++;
		UpdateConnectNumbersText ();
	}

	void OnClientDisconnect(string ip)
	{
		if (countConnectingClients > 0)
			countConnectingClients--;
		else
			countConnectingClients = 0;

		UpdateConnectNumbersText ();

		clients [ip].SetClientStatus (ListClientUI.Instance.disconnectColor);
		clients [ip].SetVideoStatus (ListClientUI.Instance.stopImage);
	}

	void OnClientInactive(string ip) // connect -> pause
	{
		clients [ip].SetClientStatus (ListClientUI.Instance.pauseColor);
		currentVideoStatus = clients [ip].videoStatus.sprite;
		clients [ip].SetVideoStatus (ListClientUI.Instance.pauseImage);
	}

	void OnClientActive(string ip) // pause -> to connect
	{
		clients [ip].SetClientStatus (ListClientUI.Instance.connectingColor);
		clients [ip].SetVideoStatus (currentVideoStatus);
	}

	void OnClientPauseVideo(string ip)
	{
		clients [ip].SetVideoStatus (ListClientUI.Instance.pauseImage);
	}

	void OnClientStopVideo(string ip)
	{
		clients [ip].SetVideoStatus (ListClientUI.Instance.stopImage);
	}

	void OnClientPlayVideo(string ip, int time)
	{
		clients [ip].SetVideoStatus (ListClientUI.Instance.playImage);
//		clients [ip].SetTimeText (time);
	}

	void OnReceiveListVideo(List<string> options)
	{
		listVideoDropdown.ClearOptions ();
		listVideoDropdown.AddOptions (options);
	}

	public void OnClickSwitchVideoUrl()
	{
		if (listVideoDropdown.options.Count == 0) {
			return;
		}
		print ("Index: " + listVideoDropdown.value);
		ServerCMDController.Instance.OnClickSwithVideoUrl (listVideoDropdown.options[listVideoDropdown.value].text);
	}

	void SetIpText(string _ip)
	{
		ipText.text = "IP: " + _ip;
	}

	void UpdateConnectNumbersText()
	{
		connectNumText.text = "Connected: " + countConnectingClients;
	}

	void StartSendVideo()
	{
		SendingPanel.SetActive (true);
	}

	void EndSendVideo()
	{
		SendingPanel.SetActive (false);
	}

}
