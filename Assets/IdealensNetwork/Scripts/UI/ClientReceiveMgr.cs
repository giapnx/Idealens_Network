using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientReceiveMgr : MonoBehaviour 
{
	public GameObject clientReceivePref;
	public Transform ContentTrans;

	Dictionary<string, ItemClientReceiveUI> clientReceives = new Dictionary<string, ItemClientReceiveUI>();

	void OnEnable ()
	{
		var clients = ServerUIController.Instance.clients;
//		print ("Test");
		foreach (var each in clients)
		{
			string ip = each.Key;
			var item = CreateNewItemReceive ();
			// Set state and index
			item.SetClientImage (clients [ip].clientStatus.color);
//			print ("SetClientImage");
			item.SetClientIndex (clients [ip].index);
			// Set process receive
			item.SetProcessReceive (0);
			clientReceives.Add (ip, item);
		}

		AsynchronousSocketListener.OnClientDisconnectEvent 	+= OnClientDisconnect;
		AsynchronousSocketListener.OnClientActiveEvent 		+= OnClientConnect;
		AsynchronousSocketListener.OnClientInactiveEvent 	+= OnClientPause;

		AsynchronousSocketListener.OnClientPercentReceive 	+= OnClinetPercentReceive;
		AsynchronousSocketListener.OnClientDoneReceiveVideo += OnClientDoneReceiveVideo;

	}

	void OnDisable()
	{
		AsynchronousSocketListener.OnClientDisconnectEvent 	-= OnClientDisconnect;
		AsynchronousSocketListener.OnClientActiveEvent 		-= OnClientConnect;
		AsynchronousSocketListener.OnClientInactiveEvent 	-= OnClientPause;

		AsynchronousSocketListener.OnClientPercentReceive 	-= OnClinetPercentReceive;
		AsynchronousSocketListener.OnClientDoneReceiveVideo -= OnClientDoneReceiveVideo;

		// Delete all item
		foreach (Transform child in ContentTrans.transform) {
			Destroy (child.gameObject);
		}
		clientReceives.Clear ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.Alpha8)) {
			CreateNewItemReceive();
		}
	}

	ItemClientReceiveUI CreateNewItemReceive()
	{
		GameObject go = Instantiate (clientReceivePref);
		go.transform.SetParent (ContentTrans);
//		print ("set transform");
		go.transform.localPosition = Vector3.zero;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localScale = Vector3.one;
		return go.GetComponent <ItemClientReceiveUI> ();
	}

	void OnClientConnect(string ip)
	{
		CheckKey (ip);

		clientReceives [ip].SetClientImage (ListClientUI.Instance.connectingColor);

	}

	void OnClientPause(string ip)
	{
		CheckKey (ip);

		clientReceives [ip].SetClientImage (ListClientUI.Instance.pauseColor);
	}

	void OnClientDisconnect(string ip)
	{
		CheckKey (ip);

		clientReceives [ip].SetClientImage (ListClientUI.Instance.disconnectColor);
	}

	void CheckKey(string key)
	{
		if (!clientReceives.ContainsKey (key)) 
		{
			Debug.Log ("Not exist key: " + key);
			return;
		}
	}

	void OnClinetPercentReceive(string ip, float percent)
	{
		clientReceives [ip].SetProcessReceive (percent);
	}

	void OnClientDoneReceiveVideo(string ip)
	{
		clientReceives [ip].SetProcessReceive (1);
	}
}
