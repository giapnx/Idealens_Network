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
	public static Hashtable clientList = new Hashtable ();

	readonly object lockObj = new object (); 
	#endregion

//	public delegate void OnClientConnect (int countClient);
//	public static event OnClientConnect OnClientConnectEvent;

	string ip = "";
	public Text ipText;
	public Text connectText;

//	public static ManualResetEvent allDone = new ManualResetEvent(false);
		
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

	void OnRequested(IAsyncResult ar) 
	{
		lock (lockObj) {
			TcpListener listener = (TcpListener)ar.AsyncState;
			TcpClient clientSocket = null;

			try
			{
				// Get client socket
				clientSocket = listener.EndAcceptTcpClient(ar);
			}
			catch (SocketException socketException) 
			{             
				Debug.Log("Socket exception: " + socketException);
				return;
			}     

			// Check 

			string clientIp = ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString();

			if (clientList.ContainsKey (clientIp))
				clientList [clientIp] = clientSocket;
			else
				clientList.Add (clientIp, clientSocket);

//			HandleClient client = new HandleClient ();
//			client.startClient (clientSocket);

			connectedTcpClients.Add(clientSocket);
			SetConnectText (connectedTcpClients.Count);

			print (clientIp + " connected | Count: " + connectedTcpClients.Count);
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
//				NetworkStream stream = client.GetStream ();
//				stream.Write (body, 0, body.Length);
//				 Detect if client disconnected
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

//public class StateObject
//{
//	// Client socket
//	public Socket workSocket = null;
//	// Size of receive buffer
//	public const int BufferSize = 1024;
//	// Receive
//}