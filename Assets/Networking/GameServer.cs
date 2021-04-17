using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    //list of all scenes possible to switch to
    public List<Scene> possibleScenes;

    //queue of actions to be done
    private Queue<Action> actions = new Queue<Action>();

    //input method the server is using
    public InputMethod inputMethod;

    //the main parent object for the vr player
    public GameObject vrMainObject;

    //camerarig prefab
    public GameObject vrCameraRig;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            SpawnRequest spawnRequest = new SpawnRequest(0, true);
            BaseRequest baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
            string message = JsonUtility.ToJson(baseRequest);
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
            tcpListener = new TcpListener(IPAddress.Any, 25565);
            tcpListener.Start();
            Debug.Log("Server is listening");
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    keepListening = true;
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
                            if (stream == null)
                                stream = connectedTcpClient.GetStream();
                            Debug.Log("connected to client");
                            
                            //queue spawning the selected player
                            actions.Enqueue(() => SpawnPlayer(inputMethod));

                            //load into the lobby scene
                            ChangeScene(0);

                            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var incommingData = new byte[length];
                                Array.Copy(bytes, 0, incommingData, 0, length);
                                // Convert byte array to string message. 							
                                clientMessage = Encoding.ASCII.GetString(incommingData);
                                //get length of message
                                int messageLength = int.Parse(clientMessage.Substring(0, 8), System.Globalization.NumberStyles.Integer);
                                Debug.Log(clientMessage.Substring(0, 8) + " from string, " + messageLength.ToString() + ", bytes: " + length.ToString());
                                //is the message shorter than the entire string?
                                if ((length - 8) > messageLength)
                                {
                                    //yes, there's multiple messages here
                                    Debug.Log("MULTIPLE MESSAGES RECEIVED");
                                    int messageStart = 8, messageEnd = messageLength;
                                    int nMessages = 1;
                                    while ((length + (nMessages * 8)) > messageEnd)
                                    {
                                        //call the message received event
                                        MessageReceived(clientMessage.Substring(messageStart, messageEnd));
                                        messageStart = messageEnd + 8;
                                        Debug.Log("Message: " + nMessages.ToString() + ": " + clientMessage.Substring(messageEnd + 1, messageEnd + 8));
                                        messageLength = int.Parse(clientMessage.Substring(messageEnd, messageEnd + 8), System.Globalization.NumberStyles.Integer);
                                        messageEnd = messageLength;
                                        nMessages++;
                                    }
                                }
                                else
                                {
                                    //call the message received event
                                    Debug.Log("SINGLE MESSAGE RECEIVED");
                                    MessageReceived(clientMessage.Substring(8));
                                }
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
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(_message.Length.ToString("00000000") + _message);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("Server Sent: '" + _message.Length.ToString("00000000") + _message + "'");
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
        Debug.Log("_message: " + _message);
        try
        {
            //get the base request object
            BaseRequest baseRequest = JsonUtility.FromJson<BaseRequest>(_message);
            switch (baseRequest.RequestType)
            {
                //the client would like to connect with an input method
                case "ConnectionRequest":
                    //get the connectionRequest object
                    ConnectionRequest connectionRequest = JsonUtility.FromJson<ConnectionRequest>(baseRequest.Request);

                    //check if the client wants to use the same input method as us
                    if (connectionRequest.requestedInput == inputMethod)
                    {
                        //we need them to disconnect, send them a disconnect request
                        DisconnectRequest disconnectRequest = new DisconnectRequest(DisconnectRequest.DisconnectReason.InputMethodPresent);
                        BaseRequest newBaseRequest = new BaseRequest(PossibleRequest.DisconnectRequest, JsonUtility.ToJson(disconnectRequest));
                        SendMessageToClient(JsonUtility.ToJson(newBaseRequest));
                        //stop listening to this connection, get a new one
                        keepListening = false;
                    }
                    //otherwise we can just let them connect.

                    break;

                //the client would like to spawn an object
                case "SpawnObject":
                    //get the request object
                    SpawnRequest spawnRequest = JsonUtility.FromJson<SpawnRequest>(baseRequest.Request);

                    //queue an object spawn(if the sender doesn't own it, we must, and vice versa)
                    actions.Enqueue(() => SpawnObject(spawnRequest.Index, !spawnRequest.SenderOwns));

                    //send a reply, but only if the sender own this object
                    if (spawnRequest.SenderOwns)
                    {
                        //copy the old request
                        BaseRequest newBaseRequest = baseRequest;
                        SpawnRequest newSpawnRequest = JsonUtility.FromJson<SpawnRequest>(newBaseRequest.Request);
                        newSpawnRequest.SenderOwns = false; //sender owns this, but we don't, so change the request slightly
                        newBaseRequest.Request = JsonUtility.ToJson(newSpawnRequest);

                        //get the request to send
                        string newRequest = JsonUtility.ToJson(newBaseRequest);
                        SendMessageToClient(newRequest);
                    }
                    break;

                //the client would like to sync an object
                case "SyncObject":
                    //get the sync request object
                    SyncRequest syncRequest = JsonUtility.FromJson<SyncRequest>(baseRequest.Request);
                    //get the serialized transform object
                    SerializableTransform serializableTransform = JsonUtility.FromJson<SerializableTransform>(syncRequest.transform);

                    //queue the transform copy
                    actions.Enqueue(() => serializableTransform.CopyToTransform(syncedObjects[syncRequest.ID].transform));
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("\"" + _message + "\" Gave Exception: " + ex.ToString());
        }
    }
    private void Start()
    {
        //disable vr
        UnityEngine.XR.XRSettings.enabled = false;
        //add our message received listener to the message received event
        MessageReceived += messageReceivedListener;
    }

    //spawns the selected player based on the chosen input method
    private void SpawnPlayer(InputMethod inputMethod)
    {
        if (inputMethod == InputMethod.NonVR)
        {
            StartCoroutine(SpawnNonVRPlayer());
        }
        else if (inputMethod == InputMethod.VR)
        {
            StartCoroutine(SpawnVRPlayer());
        }
    }

    private IEnumerator SpawnNonVRPlayer()
    {
        //spawn the playerobject
        SpawnRequest spawnRequest = new SpawnRequest(0, true);
        BaseRequest baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        string message = JsonUtility.ToJson(baseRequest);
        SendMessageToClient(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        GameObject player = GameObject.FindGameObjectWithTag("NonVRPlayer");
        //set it's position
        player.transform.localPosition = new Vector3(0.0f, 1.5f, 0.0f);

        //spawn the shadesParent 
        spawnRequest = new SpawnRequest(1, true);
        baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        message = JsonUtility.ToJson(baseRequest);
        SendMessageToClient(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        GameObject shadesParent = GameObject.FindGameObjectWithTag("NonVRShadesParent");
        //set it's parent and position
        shadesParent.transform.SetParent(player.transform);
        shadesParent.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        
        //don't destroy the player on load
        DontDestroyOnLoad(player);
    }

    private IEnumerator SpawnVRPlayer()
    {
        //spawn the camerarig
        GameObject camRig = Instantiate(vrCameraRig);

        //instantiate the main object
        GameObject mainObject = Instantiate(vrMainObject);

        //spawn the Head
        SpawnRequest spawnRequest = new SpawnRequest(2, true);
        BaseRequest baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        string message = JsonUtility.ToJson(baseRequest);
        SendMessageToClient(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        GameObject Head = GameObject.FindGameObjectWithTag("VRHead");
        //set it's parent
        Head.transform.SetParent(mainObject.transform);

        //spawn the Hands
        spawnRequest = new SpawnRequest(3, true);
        baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        message = JsonUtility.ToJson(baseRequest);
        SendMessageToClient(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        GameObject LeftHand = GameObject.FindGameObjectWithTag("VRLeftHand");
        //set it's parent
        LeftHand.transform.SetParent(mainObject.transform);

        //spawn the playerobject
        spawnRequest = new SpawnRequest(4, true);
        baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        message = JsonUtility.ToJson(baseRequest);
        SendMessageToClient(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        GameObject RightHand = GameObject.FindGameObjectWithTag("VRRightHand");
        //set it's parent
        RightHand.transform.SetParent(mainObject.transform);

        //don't destroy the player on load
        DontDestroyOnLoad(vrMainObject);

        //enable vr
        UnityEngine.XR.XRSettings.enabled = true;
    }

    public void ChangeScene(int Index)
    {
        //ask the client to change scenes
        ChangeSceneRequest changeSceneRequest = new ChangeSceneRequest(1);
        BaseRequest baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(changeSceneRequest));
        string message = JsonUtility.ToJson(baseRequest);
        SendMessageToClient(message);
        //change the scene ourselves
        SceneManager.LoadScene(possibleScenes[Index].name);
    }
}
