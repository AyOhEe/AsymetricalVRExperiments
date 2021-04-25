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
    public Dictionary<int, SyncedObject> sceneSyncedObjects = new Dictionary<int, SyncedObject>();

    //list of all spawnable objects
    public List<GameObject> spawnableObjects;

    //list of all scenes possible to switch to
    public List<Scene> possibleScenes;

    //queue of actions to be done
    private Queue<Action> actions = new Queue<Action>();

    //input method the client is using
    public InputMethod inputMethod;

    //camerarig prefab
    public GameObject vrCameraRig;

    //has the player been spawned?
    public bool playerSpawned;

    // Update is called once per frame
    void Update()
    {
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
                    string[] messages;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 							
                        clientMessage = Encoding.ASCII.GetString(incommingData);

                        //get all messages in the string
                        messages = clientMessage.Split(new string[] {",,,"}, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string message in messages)
                        {
                            //call the message received event
                            MessageReceived(message);
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
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(_message + ",,,");
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("client Sent: '" + _message + ",,," + "'");
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
                    try
                    {
                        actions.Enqueue(() => serializableTransform.CopyToTransform(syncedObjects[syncRequest.ID].transform));
                    }
                    //we don't really care if this fails for now
                    catch { }
                    break;

                //the server would like to change scenes
                case "ChangeScene":
                    //get the scene change request object
                    ChangeSceneRequest changeSceneRequest = JsonUtility.FromJson<ChangeSceneRequest>(baseRequest.Request);
                    //get the scene index
                    string sceneName = changeSceneRequest.Name;

                    //load the scene
                    actions.Enqueue(() => StartCoroutine(ChangeScene(sceneName)));
                    break;

                //the server would like to sync a scene object
                case "SceneSyncObject":
                    //get the scene sync object
                    SceneSyncRequest sceneSyncRequest = JsonUtility.FromJson<SceneSyncRequest>(baseRequest.Request);
                    //get the serializable transform
                    SerializableTransform aSerializableTransform = JsonUtility.FromJson<SerializableTransform>(sceneSyncRequest.transform);
                    //sync the object
                    try
                    {
                        actions.Enqueue(() => aSerializableTransform.CopyToTransform(sceneSyncedObjects[sceneSyncRequest.ID].transform));
                    }
                    //we don't really care if this fails for now
                    catch { }
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
        if (!playerSpawned)
        {
            playerSpawned = true;
            if (inputMethod == InputMethod.NonVR)
            {
                StartCoroutine(SpawnNonVRPlayer());
            }
            else if (inputMethod == InputMethod.VR)
            {
                StartCoroutine(SpawnVRPlayer());
            }
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
        player.transform.localPosition = new Vector3(0.0f, 2.5f, 0.0f);
    }

    private IEnumerator SpawnVRPlayer()
    {
        //enable vr
        UnityEngine.XR.XRSettings.enabled = true;
        UnityEngine.XR.XRSettings.LoadDeviceByName("OpenVR");
        Valve.VR.SteamVR.Initialize(true);
        yield return new WaitForSeconds(1);

        //spawn the object
        SpawnRequest spawnRequest = new SpawnRequest(1, true);
        BaseRequest baseRequest = new BaseRequest(PossibleRequest.SpawnObject, JsonUtility.ToJson(spawnRequest));
        string message = JsonUtility.ToJson(baseRequest);
        SendMessageToServer(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(2);
        Debug.Log("Spawning VR");
        GameObject vr = GameObject.FindGameObjectWithTag("VRHead");
    }

    private IEnumerator ChangeScene(string sceneName)
    {
        //change the scene
        SceneManager.LoadScene(sceneName);
        //wait for two frames or so
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        //get all of the synced objects that should be in the scene and spawn them in
        SceneSyncedObjectList sceneSyncedObjectList = GameObject.FindObjectOfType<SceneSyncedObjectList>();
        //make the scene syncedobject list accurate
        sceneSyncedObjects.Clear();
        foreach (GameObject g in sceneSyncedObjectList.sceneSyncedObjects)
        {
            sceneSyncedObjects.Add(sceneSyncedObjects.Count, Instantiate(g).GetComponent<SceneSyncedObject>());
        }
    }
}
