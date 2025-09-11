using UnityEngine;
using System.Net.Sockets;
using System;
using System.Text;

public class Networking : MonoBehaviour
{
    public TcpClient client = new TcpClient();
    public NetworkStream stream;
    
    public string IP;
    public int PORT;
    const double BUFFER = 8192; // 8 KB buffer size
    const int TIMEOUT = 5000;

    [System.NonSerialized]
    public byte[] data;
    
    public bool isRunning;

    public void Awake()
    {
        data = new byte[(int)BUFFER];
    }

    private void Start()
    {
        connect((bool proccess) =>
        {
            if (proccess)
            {
                isRunning = true;
                stream = client.GetStream();
                Debug.Log("The App is connected to server");
            }
            else
            {
                isRunning = false;
                Debug.Log("The App is not connected to server");
            }
        });
    }

    private void connect(Action<bool> callback)
    {
        try
        {
            bool proccess = client.ConnectAsync(IP, PORT).Wait(TIMEOUT);
            callback(proccess);
        }
        catch(Exception ex)
        {
            Debug.Log("Exception Message: " + ex.Message);
        }
    }

    public void close()
    {
        client.Close();
    }
}