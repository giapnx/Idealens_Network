using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Video;

public class TCPClient : MonoBehaviour
{
    #region private members 	
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    #endregion

    //	public InputField ipInput;
    public Canvas CanvasUI;
    public GameObject Anchor;

    [SerializeField]
    private Canvas downloadedCanvas;

    [SerializeField]
    private Slider downloadedSlider;

    [SerializeField]
    private Text downloadedText;

    [SerializeField]
    private VideoPlayer videoPlayer;

    string fileSavePath;

    //	string ip = "192.168.1.88";

    // Use this for initialization 	
    void Start()
    {
        fileSavePath = Application.persistentDataPath + "/";

       // ConnectToTcpServer();
    }

    /// <summary> 	
    /// Setup socket connection. 	
    /// </summary> 	
    public void ConnectToTcpServer()
    {
        print("Connect to Server");
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    int fileSize;
    int downloaded = 0;

    void Update()
    {
        if (fileSize > 0)
        {
            float v = (float)downloaded / fileSize;
            downloadedSlider.value = v;
            downloadedText.text = Mathf.Round(v * 100) + "%";

            if (downloaded == fileSize)
            {
                fileSize = 0;
                videoPlayer.url = fileSavePath;
                videoPlayer.Play();
                downloadedCanvas.gameObject.SetActive(false);
            }
        }
    }

    /// <summary> 	
    /// Runs in background clientReceiveThread; Listens for incomming data. 	
    /// </summary>     
    private void ListenForData()
    {
        string ip =
        //    "192.168.96.2";
         IPAddressInput.Instance.ipAddress;
        try
        {
            socketConnection = new TcpClient(ip, 8052);
            if (socketConnection.Connected)
                MainThread.Call(InActionCanvas);
            
            byte[] bytes = new byte[1024];
            int blockSize = 1024;

            while (true)
            {
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    byte[] fileNameLenByte = new byte[4];

                    stream.Read(fileNameLenByte, 0, 4);
                    int fileNameLen = BitConverter.ToInt32(fileNameLenByte, 0);
                    byte[] fileNameByte = new byte[fileNameLen];
                    stream.Read(fileNameByte, 0, fileNameLen);
                    string fileName = Encoding.ASCII.GetString(fileNameByte, 0, fileNameLen);

                    fileSavePath += fileName;
                    Debug.Log(fileSavePath);
                    
                    byte[] fileSizeLenByte = new byte[4];
                    stream.Read(fileSizeLenByte, 0, 4);

                    int fileSizeLen = BitConverter.ToInt32(fileSizeLenByte, 0);
                    byte[] fileSizeByte = new byte[4];
                    stream.Read(fileSizeByte, 0, 4);
                    fileSize = BitConverter.ToInt32(fileSizeByte, 0);
                    
                    if (File.Exists(fileSavePath))
                    {
                        File.Delete(fileSavePath);
                    }

                    if (fileSize > 0 && !File.Exists(fileSavePath))
                    {
                        int len;
                        while ((len = stream.Read(bytes, 0, blockSize)) != 0)
                        {
                            downloaded += len;
                            //  Debug.Log((float)downloaded / fileSize);

                            if (!File.Exists(fileSavePath))
                            {
                                BinaryWriter writer = new BinaryWriter(File.Open(fileSavePath, FileMode.Create));
                                writer.Write(bytes, 0, bytes.Length);
                                writer.Flush();
                                writer.Close();
                            }
                            else
                            {
                                BinaryWriter writer = new BinaryWriter(File.Open(fileSavePath, FileMode.Append));
                                writer.Write(bytes, 0, bytes.Length);
                                writer.Flush();
                                writer.Close();
                            }
                        }
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
    public void SendMessage()
    {
        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = "This is a message from one of your clients.";
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    void InActionCanvas()
    {
        downloadedCanvas.gameObject.SetActive(true);
        CanvasUI.gameObject.SetActive(false);
        Anchor.SetActive(false);
    }

    void OnApplicationQuit()
    {
        if (clientReceiveThread != null)
        {
            clientReceiveThread.Abort();
        }

        if (socketConnection != null)
        {
            socketConnection.Close();
        }
    }

}