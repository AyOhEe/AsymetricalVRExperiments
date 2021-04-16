using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class GameClient : MonoBehaviour
{
    #region private members 	
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    NetworkStream stream;
    #endregion

    //ip to connect to
    public string ConnectionIP;

    //delegate type for message received
    public delegate void MessageReceiveEvent(string _messsage);
    //called when a message is received;
    public event MessageReceiveEvent MessageReceived;


    //list of all synced objects
    public List<SyncedObject> syncedObjects;

    //list of all spawnable objects
    public List<GameObject> spawnableObjects;

    //queue of actions to be done
    private Queue<Action> actions = new Queue<Action>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string message = PossibleRequest.SpawnObject.ToString() + '"';
            message += '"' + "0";
            SendMessageToServer(message);
        }

        //run all actions queued
        if (actions.Count != 0)
        {
            lock (actions)
            {
                while (actions.Count != 0) actions.Dequeue().Invoke();
            }
        }
    }

    //spawns an object in spawnable objects from an id
    public GameObject SpawnObject(int id)
    {
        GameObject retVal = Instantiate(spawnableObjects[id]);
        return retVal;
    }

    /// <summary> 	
    /// Setup socket connection. 	
    /// </summary> 	
    public void ConnectToTcpServer()
    {
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
    /// <summary> 	
    /// Runs in background clientReceiveThread; Listens for incomming data. 	
    /// </summary>     
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient(ConnectionIP, 25565);
            while (true)
            {
                if (socketConnection == null)
                {
                    Debug.Log("socket conection null");
                    return;
                }
                try
                {			
                    //get the networkstream from socketConnection if it's null
                    if(stream == null)
                        stream = socketConnection.GetStream();
                    Debug.Log("connected to server");
                    // Read incomming stream into byte arrary. 	
                    int length;
                    Byte[] bytes = new Byte[1024];
                    string clientMessage = "";
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 							
                        clientMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("Client Received: '" + clientMessage + "'");
                        //call the message received event
                        MessageReceived(clientMessage);
                        //reset the bytes array
                        bytes = new Byte[1024];
                    }
                }
                catch (SocketException socketException)
                {
                    Debug.Log("Socket exception: " + socketException);
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void SendMessageToServer(string _message)
    {
        if (socketConnection == null)
        {
            Debug.Log("null socket connection");
            return;
        }
        if (stream == null)
        {
            Debug.Log("null stream on client");
            return;
        }

        try
        {
            //check if we can write to the socket stream
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(_message);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("client Sent: '" + _message + "'");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
    
    //listen for spawnobject requests
    void messageReceivedListener(string _message)
    {
        //split the string into sections
        string[] splitMessage = Regex.Split(_message, "\"\"");
        switch (splitMessage[0])
        {
            case "SpawnObject":
                actions.Enqueue(() => SpawnObject(int.Parse(splitMessage[1])));
                break;
            case "SyncObject":
                foreach(string s in splitMessage) { Debug.Log(s); }
                syncedObjects[int.Parse(splitMessage[1])].transform.position = StringToVector3(splitMessage[2]);
                syncedObjects[int.Parse(splitMessage[1])].transform.rotation = StringToQuaternion(splitMessage[3]);
                break;
        }
    }
    private void Start()
    {
        MessageReceived += messageReceivedListener;
    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
    public static Quaternion StringToQuaternion(string sQuaternion)
    {
        // Remove the parentheses
        if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
        {
            sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
        }

        // split the items
        string[] sArray = sQuaternion.Split(',');

        // store as a Vector3
        Quaternion result = new Quaternion(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));

        return result;
    }
}
