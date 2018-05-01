// There could be Copyright (c) of Microsoft and Yasuo Kawachi
// If there is, this code is licensed under Microsoft Limited Public License
// https://msdn.microsoft.com/en-US/cc300389 See "Exhibit B"
// I belive there is no copyright due to non creativity

using UnityEngine;
using System.Collections;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class AsynchronousSocketListener : SingletonMonoBehaviour<AsynchronousSocketListener> {

	public delegate void OnClient(string ip);
	public static event OnClient OnClientConnectEvent;
	public static event OnClient OnClientDisconnectEvent;
	public static event OnClient OnClientInactiveEvent;
	public static event OnClient OnClientActiveEvent;
	public static event OnClient OnClientPauseVideoEvent;
	public static event OnClient OnClientStopVideoEvent;

	public delegate void OnClientPlayVideo(string ip, int time);
	public static event OnClientPlayVideo OnClientPlayVideoEvent;

	public static event Action<string> OnServerStartListion;

	// Use this for initialization
	void Start () 
	{
		StartListening();
	}

	// Update is called once per frame
	void Update ()
	{
		
	}

	// State object for reading client data asynchronously
	public class StateObject 
	{
		// Client  socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 1024;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();  
	}

//	List<StateObject> activeConnections = new List<StateObject>();
	List<Socket> activeConnections = new List<Socket>();

	public void StartListening() 
	{
		IPAddress ipAddress = IPAddress.Parse(LocalIPAddress ());
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8052);

		// Create a TCP/IP socket.
		Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

		// Bind the socket to the local endpoint and listen for incoming connections.
		try {
			listener.Bind(localEndPoint);
			listener.Listen(10);

			// Start an asynchronous socket to listen for connections.
			listener.BeginAccept( new AsyncCallback(AcceptCallback),listener );
			Debug.LogFormat ("Server start listening at: {0}", ipAddress.ToString ());
			OnServerStartListion(ipAddress.ToString ());
			StartCoroutine (CheckClientsConnect (1));

		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}

	 IEnumerator CheckClientsConnect(float time)
	{
		while(true)
		{
			yield return new WaitForSeconds (time);

			if (activeConnections.Count != 0)
			{
				print ("Check connects");
				for (int i = activeConnections.Count-1; i>= 0; i--)
				{
					Socket each = activeConnections[i];
					string _ip = ((IPEndPoint)each.RemoteEndPoint).Address.ToString ();

					if (each.Poll (900, SelectMode.SelectRead) && each.Available == 0)
					{
						Debug.LogFormat ("Client: {0} disconnected", _ip);

						lock(activeConnections)
						{
							activeConnections.Remove (each);
						}

						OnClientDisconnectEvent (_ip);
					}
				}
			}
		}
	}

	public void AcceptCallback(IAsyncResult ar) {
		// Get the socket that handles the client request.
		Socket listener = (Socket) ar.AsyncState;
		Socket handler = listener.EndAccept(ar);

		// Create the state object.
		StateObject state = new StateObject();
		state.workSocket = handler;
		handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
			new AsyncCallback(ReadCallback), state);

		//確立した接続のオブジェクトをリストに追加
		string ipAddres = ((IPEndPoint)state.workSocket.RemoteEndPoint).Address.ToString();

//		if (activeConnections.Contains (state.workSocket))
//			Debug.Log ("Index: " + activeConnections.IndexOf (handler));
//		else
		activeConnections.Add (state.workSocket);

		if (OnClientConnectEvent != null) {	OnClientConnectEvent (ipAddres);}

		Debug.LogFormat ("there is {0} connections", activeConnections.Count);

		//接続待ちを再開しないと次の接続を受け入れなくなる
		listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

	}

	public void ReadCallback(IAsyncResult ar)
	{
//		print ("reading data");
		String content = String.Empty;

		// Retrieve the state object and the handler socket
		// from the asynchronous state object.
		StateObject state = (StateObject) ar.AsyncState;
		Socket handler = state.workSocket;

		// Read data from the client socket. 
		int bytesRead = handler.EndReceive(ar);

		if (bytesRead > 0) 
		{
			// There  might be more data, so store the data received so far.
			state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,bytesRead));

			// Check for end-of-file tag. If it is not there, read 
			// more data.
			content = state.sb.ToString();
//			print ("content: " + content);

			//MSDNのサンプルはEOFを検知して出力をしているけれどもncコマンドはEOFを改行時にLFしか飛ばさないので\nを追加
			if (content.IndexOf("\n") > -1 || content.IndexOf("<EOF>") > -1) {
				// All the data has been read from the 
				// client. Display it on the console.
				Debug.LogFormat("Data : {0}", content );
				// Echo the data back to the client.
				//Send(handler, content);

				string _ip = ((IPEndPoint)state.workSocket.RemoteEndPoint).Address.ToString ();
				HandlerMessage (content.Substring (0, content.Length - 5), _ip); // remove end-of-file tag

//				foreach (StateObject each in activeConnections) {
//					//string message = string.Format ("You are client No.{0}", i);
//					//					Send (each.workSocket, message);
//					//eachをactiveConnectionの中から見つけてそのインデックスを取得する方法がこれ
//					int num_of_each = activeConnections.FindIndex (delegate(StateObject s) {return s == each;});
//					//state:送信者の番号f
//					int num_of_from = activeConnections.FindIndex (delegate(StateObject s) {return s == state;});
//					Debug.LogFormat ("Index {0} | {1}", num_of_each, num_of_from);
//					string message = string.Format ("you:{0} / from:{1} / data:{2}\n", num_of_each, num_of_from, content);
////					Send (each.workSocket, message);
//				}

				//clear data in object before next receive
				//StringbuilderクラスはLengthを0にしてクリアする
				state.sb.Length = 0;

				// Not all data received. Get more.
				handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
					new AsyncCallback(ReadCallback), state);

			} else {
				// Not all data received. Get more.
				handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
					new AsyncCallback(ReadCallback), state);
			}
		}
	}

	void HandlerMessage(string _msg, string _ip)
	{
		string[] parameters = _msg.Split ("|"[0]);
		MessageType msgType = (MessageType)Enum.Parse (typeof(MessageType), parameters [0]);

		switch (msgType)
		{
		case MessageType.VIDEO_PLAY:
			OnClientPlayVideoEvent (_ip, 0);
			break;
		case MessageType.VIDEO_PAUSE:
			OnClientPauseVideoEvent (_ip);
			break;
		case MessageType.VIDEO_STOP:
			OnClientStopVideoEvent (_ip);
			break;
		case MessageType.CLIENT_ACTIVE:
			OnClientActiveEvent (_ip);
			break;
		case MessageType.CLIENT_INACTIVE:
			OnClientInactiveEvent (_ip);
			break;
		case MessageType.CLIENT_DISCONNECT:
			OnClientDisconnectEvent (_ip);
			break;
		default:
			Debug.Log ("Can't not define type of message !");
			break;
		}
	}

	public void SendMessage(string data)
	{
		foreach (Socket each in activeConnections)
		{
			Send (each, data);
		}
	}

	private void Send(Socket handler, string data)
	{
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(data);

		// Begin sending the data to the remote device.
		handler.BeginSend(byteData, 0, byteData.Length, 0,
			new AsyncCallback(SendCallback), handler);
		Debug.Log ("Send: " + data);
	}

	private void SendCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			Socket handler = (Socket) ar.AsyncState;

			// Complete sending the data to the remote device.
			int bytesSent = handler.EndSend(ar);
//			Debug.LogFormat("Sent {0} bytes to client.", bytesSent);

			//この２つはセットでつかるらしい
			//handler.Shutdown(SocketShutdown.Both);
			//handler.Close();

		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}

	private string GetIPAddress(string hostname)
	{
		IPHostEntry host;
		host = Dns.GetHostEntry(hostname);

		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
			{
				//System.Diagnostics.Debug.WriteLine("LocalIPadress: " + ip);
				return ip.ToString();
			}
		}
		return string.Empty;
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

}

public enum MessageType
{
	CLIENT_ACTIVE,
	CLIENT_INACTIVE,
	CLIENT_DISCONNECT,
	VIDEO_PLAY,
	VIDEO_PAUSE,
	VIDEO_STOP
}