using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TCPClient : SingletonMonoBehaviour<TCPClient> {  	
	#region private members 	
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread; 	
	#endregion  	

	public Canvas CanvasUI;
	public GameObject Anchor;
	public GameObject ConnectPanel;


	// Use this for initialization
	void Start () 
	{
//		Debug.Log ("Client start");
//		ConnectToTcpServer();     
	}  	
	// Update is called once per frame
//	void Update () 
//	{         
//		
//	}  	
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	public void ConnectToTcpServer ()
	{
		print ("Connect to Server");
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();  		
		} 		
		catch (Exception e) { 			
			Debug.Log("On client connect exception " + e); 		
		} 	
	}
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{ 	
		string ip = IPAddressInput.Instance.ipAddress;

		try {
			socketConnection = new TcpClient(ip, 8052);

			if (socketConnection.Connected) { MainThread.Call (OnConnectedToServer);}

			Byte[] bytes = new Byte[1024];
			while (true) {
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) 
				{
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) 
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						// Convert byte array to string message.
						string serverMessage = Encoding.ASCII.GetString(incommingData);

						String str = new String (serverMessage.ToCharArray ()); // convert to object
						MainThread.Call (ClientCommandMgr.Instance.ExecuteCommand, str);
					}
				}
			} 
		}     
		catch (SocketException socketException) 
		{             
			Debug.Log("Socket exception: " + socketException);         
		}

	}  	
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public void SendMessage(string _msg)
	{
		if (socketConnection == null) { return;}

		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite) 
			{
				print ("Send: "+_msg);
				// Convert string message to byte array.
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(_msg + "<EOF>");
				// Write byte array to socketConnection stream.
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
			}
		}
		catch (SocketException socketException) {
			Debug.Log("Socket exception: " + socketException);
		}
	}

	void OnConnectedToServer()
	{
		IPAddressInput.Instance.SaveLastIpInput ();
		CanvasUI.gameObject.SetActive (false);
		Anchor.SetActive (false);
		ConnectPanel.SetActive (false);
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			SendMessage (MessageType.CLIENT_INACTIVE.ToString ());
		}
		else
			SendMessage (MessageType.CLIENT_ACTIVE.ToString ());

//		print ("Pause: "+pauseStatus);
	}

//	void OnApplicationQuit()
//	{
//		SendMessage (MessageType.CLIENT_DISCONNECT.ToString ());
////		if (clientReceiveThread != null) 
////		{
////			clientReceiveThread.Abort ();
////		}
//	}

}