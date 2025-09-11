using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Text;

public class Events : MonoBehaviour
{
    public Networking networkBehaviour;
    public ServerSettings serverSettings;
    public ConectionStatus conectionStatus;    
    public string id = "";  // _id assigned by the server
    public string roomId = ""; // roomId assigned by the server
    public bool readingFromServer = false;
    public bool writingToServer = false;
    public bool searchingRoom = false;
    public bool searchingPlayer = false;
    public bool paired = false;
    public bool error = false;
    public bool updatingPlayerPose = false;
    public bool updatingObjectPose = false;
    public bool syncronized = false;
    public bool drawing = false;

    private static readonly string HeartbeatMessage = "{\"command\":\"HEARTBEAT\"}|";
    private static readonly byte[] HeartbeatBytes = Encoding.UTF8.GetBytes(HeartbeatMessage);
    private float nextHeartbeatTime = 0f;
    private const float heartbeatInterval = 15f; // 15 seconds interval
    
    public SearchRoom searchRoom = new SearchRoom();
    //public Drawings drawings = new Drawings();
    public PlayerPose playerPose = new PlayerPose();
    public ObjectPose objectPose = new ObjectPose();
    public GameObject playerFrame;
    public GameObject objectPrefab;
    public float VROffset = 1.15f;

    public string JSONPackage = "";
    public JsonData JSONPackageReceived = new JsonData();

    private StringBuilder _messageBuilder = new StringBuilder(); // To accumulate fragments of messages

    void Start()
    {
        
    }

    void Awake()
    {
        networkBehaviour = FindObjectOfType<Networking>();
        conectionStatus = FindObjectOfType<ConectionStatus>();

        // Check if ServerSettings gameobject exists
        GameObject serverSettingsObject = GameObject.Find("ServerSettings");
        if (serverSettingsObject != null)
        {
            //serverSettings = GameObject.Find("ServerSettings").GetComponent<ServerSettings>();
            serverSettings = serverSettingsObject.GetComponent<ServerSettings>();
            if (serverSettings != null)
            {
                networkBehaviour.IP = serverSettings.GetIP();  // Local server IP address
                networkBehaviour.PORT = serverSettings.GetPort(); // Local server PORT
            }
            else
            {
                Debug.LogError("'ServerSettings' has no attributes.");
            }
        }
        else
        {
            // Default data
            networkBehaviour.IP = "192.168.0.97"; // Default local server IP address
            networkBehaviour.PORT = 8080; // Default local server PORT
        }
        
        //conectionStatus = FindObjectOfType<ConectionStatus>();
        conectionStatus.playerIsAlone = false;
        conectionStatus.playerIsWaiting = true;
    }

    public void setSync() { syncronized = !syncronized; }

    // Receive a command from server and do ...
    public void readAction(string JsonFromServer)
    {
        // T4: timestamp when the message is received
        //long T4 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        //Debug.Log("The message from server is: " + JsonFromServer);
        
        
            // Command deserialization
            JSONPackageReceived = JsonUtility.FromJson<JsonData>(JsonFromServer);
            switch (JSONPackageReceived.getCommand())
            {       
                case "ID":
                    id = JSONPackageReceived.getID();
                    //Debug.Log("Player ID from server received");
                    //Debug.Log("My player ID is: " + id);
                    searchingRoom = true;
                    searchRoom.setCommand("SEARCH_ROOM");
                    searchRoom.setPlayerID(id);
                    JSONPackage = searchRoom.ToJson() + "|";
                    //JSONPackage = JsonUtility.ToJson(searchRoom, true);
                    //Debug.Log("My Json sent: " + JSONPackage);
                    sendRoomAction(JSONPackage);
                    JSONPackage = "{}";
                break;         

                case "WAITING_PLAYER":
                    searchingPlayer = true;
                    paired = false;
                    conectionStatus.playerIsWaiting = true;
                    //Debug.Log("Waiting player...");
                break;

                case "ROOM_CREATED":
                    roomId = JSONPackageReceived.getRoomID();
                    paired = true;
                    searchingPlayer = false;
                    searchingRoom = false;
                    conectionStatus.playerIsWaiting = false;
                    setSync();
                    //Debug.Log("Room created and players paired...");
                    //buttonManagerMod.MultiplayerGame();
                    break;

                case "DRAWING":
                    drawing = true;
                    ARDrawManager.Instance.DeserializeAndAddAnchor(JsonFromServer);//T4);
                    //Debug.Log("Other player has drown...");
                break;

                case "UPDATE_PLAYER_POSE":
                    //Debug.Log("Position: " + JSONPackageReceived.getPosition());
                    //Debug.Log("Rotation: " + JSONPackageReceived.getRotation());
                    updatingPlayerPose = true;
                    playerPose.setCommand(JSONPackageReceived.getCommand());
                    playerPose.setPlayerID(JSONPackageReceived.getID());
                    //Debug.Log("Other player update its pose:");
                break;

                case "UPDATE_OBJECT_POSE":
                    updatingObjectPose = true;
                    //Debug.Log("Updating object pose from server...");
                break;

                case "PLAYER_OFFLINE":
                    conectionStatus.playerIsAlone = true;
                    //Debug.Log("Other player is offline...");
                break;

                case "HEARTBEAT":
                    //Debug.Log("Heartbeat received from server...");
                    sendHeartbeat();
                break;

                case "RECONNECTED":
                    id = JSONPackageReceived.getID();
                    roomId = JSONPackageReceived.getRoomID();
                    paired = true;
                    searchingPlayer = false;
                    searchingRoom = false;
                    conectionStatus.playerIsWaiting = false;
                    setSync();
                break;

                default:
                    Debug.Log("No valid command...");
                break;
            }
        
    }

    // Send a serialized object to server ...
    public void sendRoomAction(string sendJson)
    {
        writingToServer = true;
        networkBehaviour.stream.BeginWrite(Encoding.UTF8.GetBytes(sendJson), 0, sendJson.Length, new AsyncCallback(endWritingProcess), networkBehaviour.stream);
        networkBehaviour.stream.Flush();
    }
    
    public void sendHeartbeat()
    {
        writingToServer = true;
        writingToServer = true;
        networkBehaviour.stream.BeginWrite(HeartbeatBytes, 0, HeartbeatBytes.Length, 
            new AsyncCallback(endWritingProcess), networkBehaviour.stream);
        networkBehaviour.stream.Flush();
    }

    public void sendAction(string sendJson)
    {
        if (writingToServer)
            return;
        try
        {
            if (!error)
            {
                writingToServer = true;
                networkBehaviour.stream.BeginWrite(Encoding.UTF8.GetBytes(sendJson), 0, sendJson.Length, new AsyncCallback(endWritingProcess), networkBehaviour.stream);
                networkBehaviour.stream.Flush();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Exception Message: " + ex.Message);
            error = true;
        }
    }

    void endWritingProcess(IAsyncResult _IAsyncResult)
    {
        writingToServer = false;
        networkBehaviour.stream.EndWrite(_IAsyncResult);
    }

    private void Update()
    {
        if(networkBehaviour.isRunning)
        {
            if(networkBehaviour.stream.DataAvailable)
            {
                readingFromServer = true;
                networkBehaviour.stream.BeginRead(networkBehaviour.data, 0, networkBehaviour.data.Length, new AsyncCallback(endReadingProcess), networkBehaviour.stream);
            }
            else
            {
                if (paired && syncronized)
                {
                    if (Time.time >= nextHeartbeatTime)
                    {
                        sendHeartbeat(); // Update heartbeat
                        //Debug.Log("Heartbeat sent to server...");
                        nextHeartbeatTime = Time.time + heartbeatInterval; // Next heartbeat time
                    }
                }
            }
        }
    }

    void endReadingProcess(IAsyncResult _IAsyncResult)
    {
        readingFromServer = false;
        int size = networkBehaviour.stream.EndRead(_IAsyncResult);
        
        // Save the received data fragment
        _messageBuilder.Append(Encoding.UTF8.GetString(networkBehaviour.data, 0, size));
        
        // Check if the message is complete (ends with '|')
        if (networkBehaviour.stream.DataAvailable) {
            networkBehaviour.stream.BeginRead(
                networkBehaviour.data, 0, networkBehaviour.data.Length,
                new AsyncCallback(endReadingProcess), networkBehaviour.stream
            );
        } else {
            // Complete message received
            string fullMessage = _messageBuilder.ToString();
            _messageBuilder.Clear();
            readAction(fullMessage);
        }
    }

    private void OnApplicationQuit()
    {
        networkBehaviour.isRunning = false;
    }
}