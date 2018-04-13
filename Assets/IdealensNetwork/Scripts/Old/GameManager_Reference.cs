using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameManager_Reference : NetworkBehaviour {

	public GameObject HostPanel;
	public GameObject ClientPanel;

	public Text ipAddressText;

	// Use this for initialization
	void Start () 
	{
		if (isServer) 
		{
			HostPanel.SetActive (true);
			ClientPanel.SetActive (false);

			string ipAddress = NetworkManager.singleton.networkAddress;
			ipAddress = (ipAddress == "localhost") ? Network.player.ipAddress : ipAddress;
			ipAddressText.text = "IP: " + ipAddress;
		} else
		{
			ClientPanel.SetActive (true);
			HostPanel.SetActive (false);
		}
	}

	public void QuitGame()
	{
		Application.Quit ();
	}

}
