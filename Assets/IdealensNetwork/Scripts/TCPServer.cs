using System;
using System.Collections; 
using System.Collections.Generic; 
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading; 
using UnityEngine;
using UnityEngine.UI;
using System.IO;  

public class TCPServer : SingletonMonoBehaviour<TCPServer> {  	
	#region private members 	
	/// <summary> 	
	/// TCPListener to listen for incomming TCP connection 	
	/// requests. 	
	/// </summary> 	
	private TcpListener tcpListener; 
	/// <summary> 
	/// Background thread for TcpServer workload. 	
	/// </summary> 	
	private Thread tcpListenerThread;  	
	/// <summary> 	
	/// Create handle to connected tcp clients. 	
	/// </summary> 	
	private List<TcpClient> connectedTcpClients = new List<TcpClient> ();

	readonly object lockObj = new object (); 
	#endregion

//	public delegate void OnClientConnect (int countClient);
//	public static event OnClientConnect OnClientConnectEvent;

	string ip = "";
	public Text ipText;
	public Text connectText;
		
	// Use this for initialization
	void Start () { 		

		ip = LocalIPAddress ();
		print (ip);

		// Start TcpServer background thread 		
		tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests)); 		
		tcpListenerThread.IsBackground = true; 		
		tcpListenerThread.Start();

		ipText.text = "IP: " + ip;
	}
	
	// Update is called once per frame
//	void Update () { 		
//		if (Input.GetKeyDown(KeyCode.Space)) {             
//			SendMessage();         
//		} 	
//	}  	
	
	/// <summary> 	
	/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
	/// </summary> 	
	private void ListenForIncommingRequests () {
		try { 			
			// Create listener.			
			tcpListener = new TcpListener(IPAddress.Parse(ip), 8052);
			tcpListener.Start();
			Debug.Log("Server is listening");

			tcpListener.BeginAcceptSocket (OnRequested, tcpListener);

//			Byte[] bytes = new Byte[1024];  			
//			while (true) { 				
//				using (connectedTcpClient = tcpListener.AcceptTcpClient()) 
//				{
//					// Get a stream object for reading 					
//					using (NetworkStream stream = connectedTcpClient.GetStream()) 
//					{ 						
//						int length; 						
//						// Read incomming stream into byte arrary. 						
//						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 							
//							var incommingData = new byte[length]; 							
//							Array.Copy(bytes, 0, incommingData, 0, length);  							
//							// Convert byte array to string message. 							
//							string clientMessage = Encoding.ASCII.GetString(incommingData); 							
//							Debug.Log("client message received as: " + clientMessage); 						
//						} 					
//					} 				
//				} 			
//			} 		
		} 		
		catch (SocketException socketException) { 			
			Debug.Log("SocketException " + socketException.ToString()); 		
		}     
	}  	
	 	
//	public void SendMessage(string msg) { 		
//		if (connectedTcpClient == null) {             
//			return;         
//		}  		
//		
//		try { 			
//			// Get a stream object for writing. 			
//			NetworkStream stream = connectedTcpClient.GetStream(); 			
//			if (stream.CanWrite) {                 
//				string serverMessage = msg;
//				// Convert string message to byte array.                 
//				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage); 				
//				// Write byte array to socketConnection stream.               
//				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);               
//				Debug.Log("Server sent message:" + msg);           
//			}       
//		} 		
//		catch (SocketException socketException) {             
//			Debug.Log("Socket exception: " + socketException);         
//		} 	
//	}

	/// <summary> 	
	/// Send message to all client using socket connection. 	
	/// </summary>
	public void SendMessage(string msg)
	{
		if (connectedTcpClients == null || connectedTcpClients.Count == 0)
			return;

		byte[] body = Encoding.ASCII.GetBytes (msg);

		for (int i = connectedTcpClients.Count -1; i >= 0; i--)
		{
			TcpClient client = connectedTcpClients [i];
			try 
			{
				// Detect if client disconnected
				if (client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0))
				{
					string Ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
					print (Ip + " has disconnected!");
					client.Close ();
					connectedTcpClients.Remove (client);
					SetConnectText (connectedTcpClients.Count);
				}
				else
				{
					NetworkStream stream = client.GetStream ();
					stream.Write (body, 0, body.Length);
				}
			} 
			catch (SocketException socketException) 
			{
				Debug.Log("Socket exception: " + socketException);
				connectedTcpClients.Remove (client);
				SetConnectText (connectedTcpClients.Count);
			}
		}
	}

	protected virtual void OnMessage(string msg){
		 Debug.Log(msg);
	}

	public string LocalIPAddress()
	{
		IPHostEntry host;
		string localIP = "";
		host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				localIP = ip.ToString();
				break;
			}
		}
		return localIP;
	}

	void OnRequested(IAsyncResult ar) 
	{
		lock (lockObj) {
			TcpListener listener = (TcpListener)ar.AsyncState;
			TcpClient client = null;
			try
			{
				// Get client socket
				client = listener.EndAcceptTcpClient(ar);
			}
			catch (SocketException socketException) 
			{             
				Debug.Log("Socket exception: " + socketException);
				return;
			}     

			// Check 
			connectedTcpClients.Add(client);
			SetConnectText (connectedTcpClients.Count);
			string Ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
			print (Ip + " connected | Count: " + connectedTcpClients.Count);
//			NetworkStream stream = client.GetStream();
//
//			StreamReader reader = new StreamReader(stream);
//
//			while (client.Connected) {
//				while (!reader.EndOfStream){
//					OnMessage(reader.ReadLine());
//				}
//
//				if (client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0)) {
//					connectedTcpClients.Remove(client);
//					break;
//				}
//			}
			listener.BeginAcceptSocket(OnRequested, listener);
		}
	}

	void SetConnectText(int _clientNumbers)
	{
		connectText.text = "Connected: " +_clientNumbers.ToString ();
	}

	void OnApplicationQuit()
	{
		if (tcpListenerThread != null) 
		{
			tcpListenerThread.Abort ();
		}
	}
}