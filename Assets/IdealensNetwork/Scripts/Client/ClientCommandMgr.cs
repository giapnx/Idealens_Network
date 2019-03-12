using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Timers;
using UnityEngine.Video;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

public class ClientCommandMgr : SingletonMonoBehaviour<ClientCommandMgr> {

//	private static ClientCommandMgr _instance;
//	public static ClientCommandMgr Instance {
//		get {
//			if (_instance == null) {
//				_instance = new GameObject ("CommandMgr").AddComponent <ClientCommandMgr> ();
//			}
//
//			return _instance;
//		}
//	}

	#region Action - Register
	event Action OnUserPressPlayHandler;
	event Action OnUserPressPauseHandler;
	event Action OnUserPressStopHandler;
	event Action OnUserPressReplayHandler;
	event Action<int> OnUserPressSwitchVideoClipHandler;
	event Action<string> OnUserPressSwitchVideoUrlHandler;

	public void RegisterUsePressPlayEvent(Action callback) 	{ OnUserPressPlayHandler 	+= callback;}
	public void RegisterUsePressPauseEvent(Action callback) { OnUserPressPauseHandler 	+= callback;}
	public void RegisterUsePressStopEvent(Action callback) 	{ OnUserPressStopHandler 	+= callback;}
	public void RegisterUsePressReplayEvent(Action callback) { OnUserPressReplayHandler += callback;}
	public void RegisterUsePressSwitchVideoClipEvent(Action<int> callback) { OnUserPressSwitchVideoClipHandler += callback;}
	public void RegisterUsePressSwitchVideoUrlEvent(Action<string> callback) { OnUserPressSwitchVideoUrlHandler += callback;}
	#endregion

	Command receivedCMD = Command.NONE;
	public enum Command
	{
		NONE,
		PLAY,
		PAUSE,
		STOP,
		REPLAY,
		SWITCH_CLIP,
		SWITCH_URL,
		GET_LIST_VIDEO
	}

//	float timeVideoCount = 0;
//	Timer aTimer = new Timer(1000);
	VideoPlayer videoPlayer;

//	void Awake()
//	{
//		_instance = this;
//	}
//
	void Start()
	{
//		aTimer.Elapsed += new ElapsedEventHandler (OnTimedEvent);
		videoPlayer = ClientVideoPlayer.Instance.videoPlayer;
		videoPlayer.loopPointReached += HandlerEndVideo;
	}

	void OnDisable()
	{
		videoPlayer.loopPointReached -= HandlerEndVideo;
	}

	// Update is called once per frame
	void Update ()
	{
//		if (videoPlayer.isPlaying)
//		{
//			timeVideoCount += Time.deltaTime;
//			aTimer.Enabled = true;
//		}
//		else
//		{
//			aTimer.Enabled = false;
//		}

//		if (TCPClient.Instance.packLength > 0 && doneReceiveFirstPack && !isLoopingPopPack) 
//		{
//			ReceiveToEndVideo ();
//		}
	}

	public void ExecuteCommand(byte[] messageByte)
	{
		byte msgType = messageByte [0];
		string msg = System.Text.Encoding.ASCII.GetString (messageByte);
		string subMsg = msg.Substring (1, msg.Length - 6); // remove first byte and flag "<EOF>"
		Debug.LogFormat ("type: {0}, msg: {1}", msgType, subMsg);

		switch (msgType)
		{
		case Message.PLAY_VIDEO:
			if (OnUserPressPlayHandler != null) { OnUserPressPlayHandler.Invoke ();	}
			TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_PLAY_VIDEO));
			break;

		case Message.PAUSE_VIDEO:
			OnUserPressPauseHandler.Invoke ();
			TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_PAUSE_VIDEO));
			break;

		case Message.STOP_VIDEO:
			OnUserPressStopHandler.Invoke ();
			TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_STOP_VIDEO));
//			timeVideoCount = 0;
			break;

		case Message.REPLAY_VIDEO:
			OnUserPressReplayHandler.Invoke ();
			TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_PLAY_VIDEO));
//			timeVideoCount = 0;
			print ("Replay !!");

			break;

		case Message.SWITCH_VIDEO_CLIP:
			if (OnUserPressSwitchVideoClipHandler != null) {
				OnUserPressSwitchVideoClipHandler (int.Parse (subMsg));
			}

			TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_STOP_VIDEO));
//			timeVideoCount = 0;
			break;

		case Message.SWITCH_VIDEO_URL:
			if (OnUserPressSwitchVideoUrlHandler != null) {
				OnUserPressSwitchVideoUrlHandler (subMsg);
			}

			TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_STOP_VIDEO));
//			timeVideoCount = 0;
			break;

		case Message.GET_LIST_VIDEO:
			string _msg2 = "";
			string path = Application.persistentDataPath + "/Video/";
			if (Directory.Exists (path))
			{
				string[] videos = Directory.GetFiles (path).Select (file => Path.GetFileName (file)).ToArray ();

				foreach (string item in videos)
				{
					_msg2 += item + "|";
				}
			}
			else
			{
				Debug.Log ("Not exist path");
				return;
			}


			TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_SEND_LIST_VIDEO, _msg2));
			break;

		default: // None
			break;
		}
	}

	int receivedBytes = 0; // number of bytes received
	int fileSize = 0; // size of file
	string currentPercent = "-1";
	string fileSavePath = "";
//	bool doneReceiveFirstPack = false;

	public void ReceiveStartOfVideo(byte[] messageByte)
	{
		print ("msgByte: " + messageByte.Length);

		currentPercent = "-1";
		receivedBytes = 0;
		int countIndex = 1; // ignore first byte - msgType

		// length of file name
		int sizeOfInt = 4; // 4 bytes
		byte[] fileNameLenByte = new byte[sizeOfInt];
		Buffer.BlockCopy (messageByte, countIndex, fileNameLenByte, 0, sizeOfInt);
		countIndex += sizeOfInt;

		// file name
		int fileNameLen = BitConverter.ToInt32(fileNameLenByte, 0);
//		print ("fileNameLen: " + fileNameLen);
		byte[] fileNameByte = new byte[fileNameLen];
		Buffer.BlockCopy (messageByte, countIndex, fileNameByte, 0, fileNameLen);
		countIndex += fileNameLen;
		string fileName = Encoding.ASCII.GetString(fileNameByte, 0, fileNameLen);
		print ("File name: " + fileName);

		fileSavePath = Application.persistentDataPath + "/Video/" + fileName;
		print (fileSavePath);

		// length of video
		byte[] fileSizeByte = new byte[sizeOfInt];
		Buffer.BlockCopy (messageByte, countIndex, fileSizeByte, 0, sizeOfInt);
		countIndex += sizeOfInt;

		// get size file
		fileSize = BitConverter.ToInt32(fileSizeByte, 0);
		if (fileSize > 0)
		{
			receivedBytes = messageByte.Length - countIndex;
			byte[] receives = new byte[receivedBytes];
			Buffer.BlockCopy (messageByte, countIndex, receives, 0, receivedBytes);

			for (int i = 0; i < 10; i++) {
				Debug.Log (receives[i]);
			}

			try
			{
				FileInfo fileInfo = new FileInfo(fileSavePath);
				if (!Directory.Exists (fileInfo.DirectoryName)) 
				{
					Directory.CreateDirectory (fileInfo.DirectoryName);
				}
				else
				{
					print ("File existed !");
					foreach (var item in Directory.GetFiles (Application.persistentDataPath + "/Video")) {
						Debug.Log (item);
					}
				}

				using (FileStream stream = new FileStream(fileSavePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
				{
					BinaryWriter writer = new BinaryWriter(stream);
					print ("receivedBytes: " + receivedBytes);
					writer.Write (receives, 0, receivedBytes);
					(writer as IDisposable).Dispose ();
					writer.Close ();
					// Send percent to server
					SendPercentReceiveToServer();
				}

			} 
			catch (Exception ex)
			{
				Debug.Log (ex);
			}

			ReceiveToEndVideoThread ();
		}
		else
		{
			Debug.LogError ("Size of file is empty !");
		}
	}

	Thread receiveToEndVideoThread = null;
	bool receiveThreadRunning = false;
	void ReceiveToEndVideoThread()
	{
		print ("Thread: receive to end video.");
		try 
		{
			receiveToEndVideoThread = new Thread( new ThreadStart(ReceiveToEndVideo));
			receiveToEndVideoThread.IsBackground = true;
			receiveThreadRunning = true;
			receiveToEndVideoThread.Start ();
		} 
		catch (Exception ex)
		{
			Debug.Log (ex);
		}
	}

	void EndThread()
	{
		if (receiveToEndVideoThread != null) 
		{
			receiveThreadRunning = false;
			receiveToEndVideoThread.Join ();
			receiveToEndVideoThread = null;
		}
	}

	public void ReceiveToEndVideo()
	{
		while (receiveThreadRunning) 
		{
			TCPClient TCPClientInstance = TCPClient.Instance;
			using(BinaryWriter writer = new BinaryWriter(File.Open(fileSavePath, FileMode.Append)))
			{
				while (!TCPClientInstance.OnReceiveDone || TCPClientInstance.receivePackVideo.Count > 0)
				{
					byte[] messageByte = TCPClient.Instance.PopPack ();
					writer.Write(messageByte, 0, messageByte.Length);
					receivedBytes += messageByte.Length;
					// Send percent to server
					SendPercentReceiveToServer ();
				}
			}

			print ("Receive: " + receivedBytes + " | " + fileSize);

			if (receivedBytes >= fileSize)
			{
				print ("Client receive done !");
				TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_DONE_RECEIVE_VIDEO));
				EndThread ();
			}
		}


	}

	void SendPercentReceiveToServer()
	{
		string percent = CalculatePercentReceive ();
		if (String.Compare (currentPercent, percent)  != 0 ) {
			TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_PERCENT_RECEIVE, percent));
			currentPercent = percent;
		}
	}

	string CalculatePercentReceive()
	{
		float percent = (float)receivedBytes / fileSize;
//		print ("percent: "+percent);
		string percentStr = percent.ToString ("F2");
		return percentStr;
	}


	void HandlerEndVideo(VideoPlayer _vp)
	{
		TCPClient.Instance.SendMessageToServer (Message.Pack (Message.CLIENT_STOP_VIDEO));
	}

	void OnApplicationPause(bool pauseStatus)
	{

	}

//	void OnTimedEvent(object source, ElapsedEventArgs e)
//	{
//		print ("time: " + Mathf.RoundToInt(timeVideoCount));
//		string msg = MessageType.VIDEO_PLAY + "|" + Mathf.RoundToInt (timeVideoCount);
//		TCPClient.Instance.SendMessage (msg);
//	}

}
