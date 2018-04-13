using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TCPClient : MonoBehaviour {  	
	#region private members 	
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread; 	
	#endregion  	

//	public InputField ipInput;
	public Canvas CanvasUI;
	public GameObject Anchor;

//	string ip = "192.168.1.88";

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

			if (socketConnection.Connected) { MainThread.Call (InActionCanvas);}

			Byte[] bytes = new Byte[1024];
			while (true) { 				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData); 						
//						Debug.Log("server message received as: " + serverMessage);

						String str = new String (serverMessage.ToCharArray ());
						MainThread.Call (CommandMgr.Instance.ExecuteCommand, str);

					} 
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     

	}  	
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public void SendMessage() {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				string clientMessage = "This is a message from one of your clients."; 				
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Client sent his message - should be received by server");             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);
		}     
	}

	void InActionCanvas()
	{
		CanvasUI.gameObject.SetActive (false);
		Anchor.SetActive (false);
	}

//	void OnApplicationQuit()
//	{
//		if (clientReceiveThread != null) 
//		{
//			clientReceiveThread.Abort ();
//		}
//	}

}