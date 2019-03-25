using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient : SingletonMonoBehaviour<TCPClient>
{
    #region private members 	
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    #endregion

    public Canvas CanvasUI;
    public GameObject Anchor;

    public GameObject ConnectPanel;
    public GameObject TryConnectPanel;

    private int _maxPack = 1000;
    public Queue<byte[]> receivePackVideo = new Queue<byte[]>();
    static object _door = new object();
    public bool OnReceiveDone = true;
    public float timeWaitConnect = 5;
    const int RECONNECT_ATTEMPTS = 1;
    int reconnectionsDone = 0;

    Byte[] bytes = new Byte[1024];

    static Socket client;
    static string ipString = null;

    // Use this for initialization
    void Start()
    {
        TryStartClient();
    }

    // Test func
    public void DisconnectClient()
    {
        if (client != null && client.Connected)
        {
            try
            {
                client.Close();
                Debug.Log("Close socket !");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        StartCoroutine(CheckTryConnect(10));
    }
    // End Test

    public void TryStartClient()
    {
        ConnectToTcpServer();
        StartCoroutine(CheckTryConnect(3));
    }

    IEnumerator CheckTryConnect(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (!client.Connected)
        {
            ActivePanelConnect();
        }
    }

    public void StartClient()
    {
        // Connect to a remote device.
        try
        {
            // Create a TCP/IP socket.
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            client.BeginConnect(ipString, 8052, new AsyncCallback(ConnectCallBack), client);

        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            client.Close();
            if (reconnectionsDone >= RECONNECT_ATTEMPTS)
            {
                MainThread.Call(ActivePanelConnect);
            }
            else
            {
                Thread.Sleep((int)(timeWaitConnect * 1000));
                ++reconnectionsDone;
                StartClient();
            }
        }
    }

    void ConnectCallBack(IAsyncResult ar)
    {
        try
        {
            // Complete the connection.
            client.EndConnect(ar);
            reconnectionsDone = 0;

            if (client.Connected) { MainThread.Call(OnConnectedToServer); }

            Receive(client);
        }
        catch (Exception ex)
        {
            Debug.Log("ConnectCallBack: " + ex);
            if (reconnectionsDone >= RECONNECT_ATTEMPTS)
            {
                Debug.Log("re-enabling connect panel");
                MainThread.Call(ActivePanelConnect);
            }
            else
            {
                Thread.Sleep((int)(timeWaitConnect * 1000));
                ++reconnectionsDone;
                StartClient();
            }
        }
    }

    void Receive(Socket client)
    {
        print("Start receive data from remote device.");
        try
        {
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
        }
        catch (Exception ex)
        {
            Debug.Log("Receive: " + ex);
        }
    }

    void ReceiveCallBack(IAsyncResult ar)
    {
        print("ReceiveCallBack");
        String content = String.Empty;

        try
        {
            // Receive the state object and the client socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;

            // Read data from the remote device.
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                byte[] pack = new byte[bytesRead];
                Buffer.BlockCopy(state.buffer, 0, pack, 0, bytesRead);

                byte _msgType = state.buffer[0];
                Debug.Log("type: " + _msgType);

                if (_msgType != Message.SEND_VIDEO)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    content = state.sb.ToString();
                    print("ReceiveCallBack: " + content);

                    if (content.IndexOf("<EOF>") > -1) // exist flag EOF.
                    {

                        byte[] msg = new byte[bytesRead];
                        Buffer.BlockCopy(state.buffer, 0, msg, 0, bytesRead);
                        MainThread.Call(ClientCommandMgr.Instance.ExecuteCommand, msg);

                        state.sb.Length = 0;
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
                    }
                    else
                    {
                        // Get more data.
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveToEndCallBack), state);
                    }
                }
                else
                {
                    //					for (int i = 1; i < 15; i++) {
                    //						Debug.Log (pack[i]);
                    //					}
                    //					print ("bytesRead: "+bytesRead);
                    //					print (Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    OnReceiveDone = false;
                    MainThread.Call(ClientCommandMgr.Instance.ReceiveStartOfVideo, pack);
                    // Get more data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveToEndVideoCallBack), state);
                }


            }
        }
        catch (Exception ex)
        {
            Debug.Log("ReceiveCallBack: " + ex);
            client.Close();
            Thread.Sleep((int)(timeWaitConnect * 1000));
            StartClient();
        }
    }

    void ReceiveToEndCallBack(IAsyncResult ar)
    {
        print("ReceiveToEndCallBack");
        String content = String.Empty;

        try
        {
            StateObject state = (StateObject)ar.AsyncState;

            // Read data from the remote device.
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                content = state.sb.ToString();
                print("ReceiveToEndCallBack: " + content);

                if (content.IndexOf("<EOF>") > -1) // exist flag EOF.
                {
                    print("bytesRead: " + bytesRead);
                    byte[] msg = new byte[bytesRead];
                    Buffer.BlockCopy(state.buffer, 0, msg, 0, bytesRead);
                    MainThread.Call(ClientCommandMgr.Instance.ExecuteCommand, msg);
                    state.sb.Length = 0;
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
                }
                else
                {
                    // Get more data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveToEndCallBack), state);
                }

            }
        }
        catch (Exception ex)
        {
            Debug.Log("ReceiveToEndCallBack: " + ex);
            client.Close();
            Thread.Sleep((int)(timeWaitConnect * 1000));
            StartClient();
        }
    }

    void ReceiveToEndVideoCallBack(IAsyncResult ar)
    {
        //		print ("ReceiveToEndVideoCallBack");
        String content = String.Empty;

        try
        {
            StateObject state = (StateObject)ar.AsyncState;

            // Read data from the remote device.
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);

                //				print ("bytesRead: " + bytesRead + " | ReceiveToEndVideoCallBack: " + content);

                if (content.IndexOf("<EOF>") > -1) // exist flag EOF.
                {
                    byte[] msg = new byte[bytesRead - 5]; // remove last 5 bytes flag
                    Buffer.BlockCopy(state.buffer, 0, msg, 0, bytesRead - 5);
                    //					MainThread.Call(ClientCommandMgr.Instance.ReceiveToEndVideo, msg);
                    //					MainThread.Call (PushPack, msg);
                    PushPack(msg);
                    OnReceiveDone = true;
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallBack), state);
                    print("Done receive !!!!!!");
                }
                else
                {
                    //					MainThread.Call (PushPack, state.buffer);
                    byte[] msg = new byte[bytesRead]; // remove last 5 bytes flag
                    Buffer.BlockCopy(state.buffer, 0, msg, 0, bytesRead);
                    PushPack(msg);
                    // Get more data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveToEndVideoCallBack), state);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            client.Close();
            Thread.Sleep((int)(timeWaitConnect * 1000));
            StartClient();
        }
    }

    public void SendMessageToServer(byte[] byteData)
    {
        if (client == null || !client.Connected) { return; }

        client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), client);
    }

    void SendCallBack(IAsyncResult ar)
    {
        try
        {
            // Complete sendung the data to the remote device.
            int byteSent = client.EndSend(ar);
        }
        catch (Exception ex)
        {
            Debug.Log("SendCallBack: " + ex);
            if (reconnectionsDone >= RECONNECT_ATTEMPTS)
            {
                Debug.Log("re-enabling connect panel");
                MainThread.Call(ActivePanelConnect);
            }
            else
            {
                Thread.Sleep((int)(timeWaitConnect * 1000));
                ++reconnectionsDone;
                StartClient();
            }
        }
    }

    /// <summary> 	
    /// Setup socket connection. 	
    /// </summary> 	
    public void ConnectToTcpServer()
    {
        ipString = IPAddressInput.Instance.ipAddress;
        print("Connect to Server");
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(StartClient));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    public byte[] PopPack()
    {
        byte[] result;
        lock (_door)
        {
            while (receivePackVideo.Count == 0)
            {
                Monitor.Wait(_door);
            }

            result = receivePackVideo.Dequeue();
            Monitor.Pulse(_door);

            //			print ("--- Pop: " + result[0]);
        }

        return result;
    }

    public void PushPack(byte[] pack)
    {
        lock (_door)
        {
            while (receivePackVideo.Count >= _maxPack)
            {
                Monitor.Wait(_door);
            }
            //			print ("+++ Push: " + pack[0]);
            receivePackVideo.Enqueue(pack);
            Monitor.Pulse(_door);
        }
    }

    //	public void ClearPack()
    //	{
    //		receivePackVideo.Clear ();
    //		packLength = 0;
    //	}

    void OnConnectedToServer()
    {
        CanvasUI.gameObject.SetActive(false);
        Anchor.SetActive(false);
        IPAddressInput.Instance.SaveLastIpInput();
    }

    void ActivePanelConnect()
    {
        CanvasUI.gameObject.SetActive(true);
        Anchor.SetActive(true);
        ConnectPanel.SetActive(true);
        TryConnectPanel.SetActive(false);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SendMessageToServer(Message.Pack(Message.CLIENT_INACTIVE));
        }
        else
        {
            SendMessageToServer(Message.Pack(Message.CLIENT_ACTIVE));

            // Send videoState
            switch (ClientVideoPlayer.Instance.currentVideoState)
            {
                case VideoState.PLAYING:
                    SendMessageToServer(Message.Pack(Message.CLIENT_PLAY_VIDEO));
                    break;
                case VideoState.PAUSED:
                    SendMessageToServer(Message.Pack(Message.CLIENT_PAUSE_VIDEO));
                    break;
                case VideoState.STOPPED:
                    SendMessageToServer(Message.Pack(Message.CLIENT_STOP_VIDEO));
                    break;
                default:
                    break;
            }
        }

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