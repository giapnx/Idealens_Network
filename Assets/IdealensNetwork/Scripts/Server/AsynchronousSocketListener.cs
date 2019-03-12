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
using System.IO;

public class AsynchronousSocketListener : SingletonMonoBehaviour<AsynchronousSocketListener> {

	public delegate void OnClient(string ip);
	public static event OnClient OnClientConnectEvent;
	public static event OnClient OnClientDisconnectEvent;
	public static event OnClient OnClientInactiveEvent;
	public static event OnClient OnClientActiveEvent;
	public static event OnClient OnClientPauseVideoEvent;
	public static event OnClient OnClientStopVideoEvent;
	public static event OnClient OnClientDoneReceiveVideo;
	public static event Action<string, float> OnClientPercentReceive;

	public delegate void OnClientPlayVideo(string ip, int time);
	public static event OnClientPlayVideo OnClientPlayVideoEvent;

	public static event Action<string> OnServerStartListening;
	public static event Action<List<string>> OnRecieveListVideo;
	bool isReceiveListVideo = false;

	public static event Action OnStartSendVideo;

	// Use this for initialization
	void Start () 
	{
		StartListening();
	}

	// Update is called once per frame
//	void Update ()
//	{
//		
//	}

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

			OnServerStartListening(ipAddress.ToString ());
			StartCoroutine (CheckClientsConnect (2.5f));

		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}

	/// <summary>
	/// Checks the connect status of clients by time.
	/// </summary>
	/// <returns>The clients connect.</returns>
	/// <param name="time">Loop Time</param>
	 IEnumerator CheckClientsConnect(float time)
	{
		while(true)
		{
			yield return new WaitForSeconds (time);

			if (activeConnections.Count != 0)
			{
//				print ("Check connects");
				for (int i = activeConnections.Count-1; i>= 0; i--)
				{
					Socket each = activeConnections[i];
					string _ip = ((IPEndPoint)each.RemoteEndPoint).Address.ToString ();

					if (each.Poll ((int)(time * 1000) - 200, SelectMode.SelectRead) && each.Available == 0)
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

		activeConnections.Add (state.workSocket);
		// Get lists video from first connect client
		if (!isReceiveListVideo) { Send (state.workSocket, Message.Pack (Message.GET_LIST_VIDEO));}

		if (OnClientConnectEvent != null) {	OnClientConnectEvent (ipAddres);}

		Debug.LogFormat ("there is {0} connections", activeConnections.Count);

		//接続待ちを再開しないと次の接続を受け入れなくなる
		listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
	}

	public void ReadCallback(IAsyncResult ar)
	{
		String content = String.Empty;
		try 
		{
			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			StateObject state = (StateObject) ar.AsyncState;
			Socket handler = state.workSocket;

			// Read data from the client socket. 
			int bytesRead = handler.EndReceive(ar);

			if (bytesRead > 0)
			{
				print ("bytesRead: "+bytesRead);
				byte _msgType = state.buffer [0];
//				Debug.Log ("msgType: " + _msgType);
				// There  might be more data, so store the data received so far.
//				state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

				// Check for end-of-file tag. If it is not there, read more data.
//				content = state.sb.ToString();
				content = Encoding.ASCII.GetString(state.buffer, 1, bytesRead-1); // remove first byte
//				print ("content: " + content);

				//MSDNのサンプルはEOFを検知して出力をしているけれどもncコマンドはEOFを改行時にLFしか飛ばさないので\nを追加
				if (content.IndexOf("\n") > -1 || content.IndexOf("<EOF>") > -1) {
					// All the data has been read from the 
					// client. Display it on the console.
					Debug.LogFormat("Data : {0}", content );

					string _ip = ((IPEndPoint)state.workSocket.RemoteEndPoint).Address.ToString ();
					string[] result = content.Split (new string[]{ "<EOF>" }, System.StringSplitOptions.None);
					HandlerMessage (_msgType, result[0], _ip); // remove flag "<EOF>"

					//clear data in object before next receive
					//StringbuilderクラスはLengthを0にしてクリアする
//					state.sb.Length = 0;

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
		catch (Exception ex) 
		{
			Debug.Log (ex);
		}

	}

	void HandlerMessage(byte msgType, string _msg, string _ip)
	{
		switch (msgType)
		{
		case Message.CLIENT_PLAY_VIDEO:
			OnClientPlayVideoEvent (_ip, 0);
			break;

		case Message.CLIENT_PAUSE_VIDEO:
			OnClientPauseVideoEvent (_ip);
			break;

		case Message.CLIENT_STOP_VIDEO:
			OnClientStopVideoEvent (_ip);
			break;

		case Message.CLIENT_ACTIVE:
			OnClientActiveEvent (_ip);
			break;

		case Message.CLIENT_INACTIVE:
			OnClientInactiveEvent (_ip);
			break;

		case Message.CLIENT_DISCONNECT:
			OnClientDisconnectEvent (_ip);
			break;

		case Message.CLIENT_SEND_LIST_VIDEO:
			string[] parameters = _msg.Substring (0, _msg.Length - 1).Split ("|"[0]);
			List<string> options = new List<string> (parameters);

			OnRecieveListVideo (options);
			isReceiveListVideo = true;
			break;

		case Message.CLIENT_PERCENT_RECEIVE:
			print ("percent: " + _msg);
			float percent = 0;
			if (float.TryParse (_msg, out percent))
				OnClientPercentReceive (_ip, percent);
			break;

		case Message.CLIENT_DONE_RECEIVE_VIDEO:
			OnClientDoneReceiveVideo (_ip);
			break;

		default:
			Debug.Log ("Can't not define type of message !");
			break;
		}
	}

	/// <summary>
	/// Send a string message to all of clients.
	/// </summary>
	/// <param name="data">messageStr.</param>
	public void SendAll(string messageStr)
	{
		foreach (Socket each in activeConnections)
		{
			Send (each, messageStr);
		}
	}

	/// <summary>
	/// Send a byte array message to all of clients.
	/// </summary>
	/// <param name="byteData">Byte data.</param>
	public void SendAll(byte[] byteData)
	{
		foreach (Socket each in activeConnections) 
		{
			Send (each, byteData);
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

	void Send(Socket handler, byte[] byteData)
	{
		// Begin sending the data to the remote device.
		handler.BeginSend(byteData, 0, byteData.Length, 0,
			new AsyncCallback(SendCallback), handler);
		Debug.Log ("Send: " + byteData.Length + " bytes.");
	}

	public void SendVideoToClient(string nameOfVideo)
	{
//		string path = Path.Combine (Application.dataPath + "/IdealensNetwork/Video/", nameOfVideo);
		string path = Path.Combine (Application.persistentDataPath + "/Video/", nameOfVideo);
		if (!File.Exists (path))
		{
			Debug.LogError ("Not exist video !");
			return;
		}

		OnStartSendVideo ();

		byte msgType = Message.SEND_VIDEO;

		// Convert data of video to bytes
		byte[] fileNameByte = Encoding.ASCII.GetBytes (nameOfVideo);
		byte[] fileNameLenghtByte = BitConverter.GetBytes (fileNameByte.Length); // 4 bytes

		byte[] dataOfVideo = File.ReadAllBytes (path);
		print ("Length: " + dataOfVideo.Length);

		byte[] videoSizeByte = BitConverter.GetBytes (dataOfVideo.Length);

		byte[] eofByte = System.Text.Encoding.ASCII.GetBytes ("<EOF>");

		byte[] dataOfMsg = new byte[1 + fileNameByte.Length + fileNameLenghtByte.Length
			+ dataOfVideo.Length + videoSizeByte.Length + eofByte.Length];

		int countData = 0;
		// type of msg
		dataOfMsg [0] = msgType;

		// name of file
		fileNameLenghtByte.CopyTo (dataOfMsg, countData += 1); // 4 bytes
		fileNameByte.CopyTo (dataOfMsg, countData += fileNameLenghtByte.Length);
		// data of video
		videoSizeByte.CopyTo (dataOfMsg, countData += fileNameByte.Length); // 4 bytes
		dataOfVideo.CopyTo (dataOfMsg, countData += videoSizeByte.Length);
		eofByte.CopyTo (dataOfMsg, countData += dataOfVideo.Length);



		print ("dataOfMsg: " + dataOfMsg.Length + " | Last byte: " + dataOfMsg[dataOfMsg.Length-1]);

		int numberOfPack = dataOfMsg.Length / 1024000;
		int tailBytes = dataOfMsg.Length % 1024000;

//		foreach (var each in activeConnections) 
//		{
//			each.BeginSend (dataOfMsg, 0, dataOfMsg.Length, 0, new AsyncCallback(SendCallback), each);
//		}

		for (int i = 0; i < numberOfPack; i++) {
			foreach (var each in activeConnections)
			{
				each.BeginSend (dataOfMsg, i*1024000, 1024000, 0, new AsyncCallback(SendCallback), each);
			}
		}

		if (tailBytes != 0)
		{
			foreach (var each in activeConnections)
			{
				each.BeginSend (dataOfMsg, numberOfPack*1024000, tailBytes, 0, new AsyncCallback(SendCallback), each);
			}
		}

	}

	private void SendCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			Socket handler = (Socket) ar.AsyncState;

			// Complete sending the data to the remote device.
			int bytesSent = handler.EndSend(ar);
			Debug.LogFormat("Sent {0} bytes to client.", bytesSent);

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
	VIDEO_STOP,
	LIST_VIDEO,
	GET_LIST_VIDEO,
	SEND_VIDEO
}

// State object for reading client data asynchronously
public class StateObject 
{
	// Client  socket.
	public Socket workSocket = null;
	// Size of receive buffer.
	public const int BufferSize = 10240;
	// Receive buffer.
	public byte[] buffer = new byte[BufferSize];
	// Received data string.
	public StringBuilder sb = new StringBuilder();
}