using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public Dictionary<int, SyncedObject> syncedObjects = new Dictionary<int, SyncedObject>();

    //list of all spawnable objects
    public List<GameObject> spawnableObjects;

    //list of all scenes possible to switch to
    public List<Scene> possibleScenes;

    //queue of actions to be done
    private Queue<Action> actions = new Queue<Action>();

    //input method the client is using
    public InputMethod inputMethod;

    //the main parent object for the vr player
    public GameObject vrMainObject;

    //camerarig prefab
    public GameObject vrCameraRig;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnRequest spawnRequest = new SpawnRequest(0, true);
            BaseRequest baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
            string message = JsonUtility.ToJson(baseRequest);
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
    public GameObject SpawnObject(int id, bool localSpawn)
    {
        GameObject retVal = Instantiate(spawnableObjects[id]);
        retVal.GetComponent<SyncedObject>().localOwned = localSpawn;
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

                    //send the connection request to ensure there's only one of each input method on a server
                    ConnectionRequest connectionRequest = new ConnectionRequest(inputMethod);
                    BaseRequest baseRequest = new BaseRequest(PossibleRequest.ConnectionRequest, JsonUtility.ToJson(connectionRequest));
                    SendMessageToServer(JsonUtility.ToJson(baseRequest));

                    //queue spawning the selected player
                    actions.Enqueue(() => SpawnPlayer(inputMethod));

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
                            while ((length - (nMessages * 8)) > messageEnd)
                            {
                                //call the message received event
                                MessageReceived(clientMessage.Substring(messageStart, messageEnd));
                                messageStart = messageEnd + 8;
                                Debug.Log(clientMessage.Substring(messageEnd + 1, messageEnd + 8));
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
                //the server wants us to disconnect
                case "DisconnectRequest":
                    //get the request object
                    DisconnectRequest disconnectRequest = JsonUtility.FromJson<DisconnectRequest>(baseRequest.Request);
                    //get the reason why and log it
                    DisconnectRequest.DisconnectReason disconnectReason = disconnectRequest.disconnectReason;
                    Debug.LogWarning("Asked to disconnect: " + disconnectReason.ToString());
                    //queue the destruction of the client object for cleanup
                    actions.Enqueue(() => Destroy(gameObject));
                    break;

                //the server would like to spawn an object
                case "SpawnObject":
                    //get the request object
                    SpawnRequest spawnRequest = JsonUtility.FromJson<SpawnRequest>(baseRequest.Request);

                    //queue an object spawn(if the sender doesn't own it, we must)
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
                        SendMessageToServer(newRequest);
                    }
                    break;

                //the server would like to sync an object
                case "SyncObject":
                    //get the sync request object
                    SyncRequest syncRequest = JsonUtility.FromJson<SyncRequest>(baseRequest.Request);
                    //get the serialized transform object
                    SerializableTransform serializableTransform = JsonUtility.FromJson<SerializableTransform>(syncRequest.transform);

                    //queue the transform copy
                    actions.Enqueue(() => serializableTransform.CopyToTransform(syncedObjects[syncRequest.ID].transform));
                    break;

                //the server would like to change scenes
                case "ChangeScene":
                    //get the scene change request object
                    ChangeSceneRequest changeSceneRequest = JsonUtility.FromJson<ChangeSceneRequest>(baseRequest.Request);
                    //get the scene index
                    int sceneIndex = changeSceneRequest.Index;

                    //load the scene
                    SceneManager.LoadScene(possibleScenes[sceneIndex].name);
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
        SendMessageToServer(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        GameObject player = GameObject.FindGameObjectWithTag("NonVRPlayer");
        //set it's position
        player.transform.localPosition = new Vector3(0.0f, 1.5f, 0.0f);

        //spawn the shadesParent 
        spawnRequest = new SpawnRequest(1, true);
        baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        message = JsonUtility.ToJson(baseRequest);
        SendMessageToServer(message);
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
        SendMessageToServer(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        GameObject Head = GameObject.FindGameObjectWithTag("VRHead");
        //set it's parent
        Head.transform.SetParent(mainObject.transform);

        //spawn the Hands
        spawnRequest = new SpawnRequest(3, true);
        baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        message = JsonUtility.ToJson(baseRequest);
        SendMessageToServer(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        GameObject LeftHand = GameObject.FindGameObjectWithTag("VRLeftHand");
        //set it's parent
        LeftHand.transform.SetParent(mainObject.transform);

        //spawn the playerobject
        spawnRequest = new SpawnRequest(4, true);
        baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        message = JsonUtility.ToJson(baseRequest);
        SendMessageToServer(message);
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
}
