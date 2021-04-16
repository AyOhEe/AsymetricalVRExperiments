using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

//request types
public enum PossibleRequest
{
    SyncObject,
    SpawnObject
}

public class GameServer : MonoBehaviour
{
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
    /// Create handle to connected tcp client. 	
    /// </summary> 	
    private TcpClient connectedTcpClient;
    #endregion

    //the ip to host the server on
    public string HostIP;
    //the networkStream from connectedTcpClient
    NetworkStream stream;

    //delegate type for message received
    public delegate void MessageReceiveEvent(string _messsage);
    //called when a message is received;
    public event MessageReceiveEvent MessageReceived;

    //should the servere continue listening?
    public bool keepListening = true;

    //list of all synced objects
    public Dictionary<int, SyncedObject> syncedObjects = new Dictionary<int, SyncedObject>();

    //list of all spawnable objects
    public List<GameObject> spawnableObjects;

    //queue of actions to be done
    private Queue<Action> actions = new Queue<Action>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            string message = PossibleRequest.SpawnObject.ToString() + '"';
            message += '"' + "0" + '"';
            message += '"' + false.ToString();
            SendMessageToClient(message);
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
    public GameObject SpawnObject(int id, bool localSpawn)
    {
        GameObject retVal = Instantiate(spawnableObjects[id]);
        retVal.GetComponent<SyncedObject>().localOwned = localSpawn;
        return retVal;
    }

    // Use this for initialization
    public void StartServer()
    {
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    /// </summary> 	
    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on localhost port 25565. 			
            tcpListener = new TcpListener(IPAddress.Parse(HostIP), 25565);
            tcpListener.Start();
            Debug.Log("Server is listening");
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    while (keepListening)
                    {
                        if (connectedTcpClient == null)
                        {
                            Debug.Log("null connected client");
                            return;
                        }

                        try
                        {
                            // Read incomming stream into byte arrary. 	
                            int length;
                            string clientMessage = "";
                            //get stream if it's null
                            if(stream == null)
                                stream = connectedTcpClient.GetStream();
                            Debug.Log("connected to client");
                            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var incommingData = new byte[length];
                                Array.Copy(bytes, 0, incommingData, 0, length);
                                // Convert byte array to string message. 							
                                clientMessage = Encoding.ASCII.GetString(incommingData);
                                Debug.Log("Server Received: '" + clientMessage + "'");
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
                stream = null;
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    //sends _message to the client if there is a stream present
    public void SendMessageToClient(string _message)
    {
        if (connectedTcpClient == null)
        {
            Debug.Log("null connected client");
            return;
        }
        if (stream == null)
        {
            Debug.Log("null stream on server");
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
                Debug.Log("Server Sent: '" + _message + "'");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    //listen for requests
    void messageReceivedListener(string _message)
    {
        //split the string into sections
        string[] splitMessage = Regex.Split(_message, "\"\"");
        switch (splitMessage[0])
        {
            //the client would like to spawn an object
            case "SpawnObject":
                //queue an object spawn
                actions.Enqueue(() => SpawnObject(int.Parse(splitMessage[1]), bool.Parse(splitMessage[2])));
                //was this actually just the response to us?
                if(!bool.Parse(splitMessage[2]))
                    SendMessageToClient(splitMessage[0] + "\"\"" + splitMessage[1] + "\"\"" + true.ToString()); //yes, say as such in OUR response
                break;
            //the client would like to sync an object
            case "SyncObject":
                //queue rotation being set
                actions.Enqueue(() => syncedObjects[int.Parse(splitMessage[1])].transform.position = StringToVector3(splitMessage[2]));
                //queue rotation being set
                actions.Enqueue(() => syncedObjects[int.Parse(splitMessage[1])].transform.rotation = StringToQuaternion(splitMessage[3]));
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
