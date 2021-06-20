using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiClient : MonoBehaviour
{
    //ip to connect to
    public string ConnectionIP;
    //the socket connected to the server
    Socket sender;

    //delegate type for message received
    public delegate void MessageReceiveEvent(string _messsage);
    //called when a message is received;
    public event MessageReceiveEvent MessageReceived;

    //dicts of all synced objects
    public SerializableDictionary<int, MultiSyncedObject> syncedObjects = new SerializableDictionary<int, MultiSyncedObject>();
    public SerializableDictionary<int, MultiSyncedObject> sceneSyncedObjects = new SerializableDictionary<int, MultiSyncedObject>();
    public int syncedObjectsTotal = 0;
    //list of all spawnable objects
    public List<GameObject> spawnableObjects;
    //list of all scenes possible to switch to
    public List<Scene> possibleScenes;

    //queue of actions to be done
    private Queue<Action> actions = new Queue<Action>();

    //input method the client is using
    public InputMethod inputMethod;
    //has the player been spawned?
    public bool playerSpawned;

    public bool hostAuthority;
    public string currentScene;

    private GameObject lastSyncedObjectSpawned;
    private int spawnsWithoutInstantiate;

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
    public GameObject SpawnObject(int index)
    {
        GameObject instance = Instantiate(spawnableObjects[index]);
        MultiSyncedObject syncedObject = instance.GetComponent<MultiSyncedObject>();
        syncedObject.doSyncedObjectsDictSetup = true;
        syncedObject.index = index;
        return instance;
    }
    //spawns an object in spawnable objects from an id
    public GameObject SpawnObjectWithoutSetup(int index, int ID)
    {
        if (index == -1)
        {
            List<MultiSyncedObject> lastSpawnChildren = new List<MultiSyncedObject>();
            lastSyncedObjectSpawned.GetComponentsInChildren<MultiSyncedObject>(true, lastSpawnChildren);

            MultiSyncedObject current = lastSpawnChildren[spawnsWithoutInstantiate++];
            current.doSyncedObjectsDictSetup = false;
            current.ID = ID;

            return current.gameObject;
        }
        else
        {
            GameObject instance = Instantiate(spawnableObjects[index]);
            MultiSyncedObject syncedObject = instance.GetComponent<MultiSyncedObject>();
            syncedObject.doSyncedObjectsDictSetup = false;
            syncedObject.index = index;
            syncedObject.ID = ID;
            lastSyncedObjectSpawned = instance;
            return instance;
        }
    }

    //spawns a local object
    public GameObject LocalSpawnObject(int index)
    {
        GameObject instance = Instantiate(spawnableObjects[index]);
        MultiSyncedObject syncedObject = instance.GetComponent<MultiSyncedObject>();
        syncedObject.localOwned = true;
        syncedObject.index = index;
        syncedObjects.Add(syncedObjectsTotal, syncedObject);

        MultiSpawnRequest spawnRequest = new MultiSpawnRequest(index);
        MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSpawnObject, JsonUtility.ToJson(spawnRequest));
        SendMessageToServer(JsonUtility.ToJson(baseRequest));
        return instance;
    }

    /// <summary> 	
    /// Setup socket connection. 	
    /// </summary> 	
    public void ConnectToTcpServer()
    {
        try
        {
            Thread clientReceiveThread = new Thread(new ThreadStart(ListenForData));
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
        byte[] bytes = new byte[1024];

        // Connect to a Remote server  
        // Get Host IP Address that is used to establish a connection   
        IPAddress ipAddress = IPAddress.Parse(ConnectionIP);
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

        // Create a TCP/IP  socket.    
        sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Connect the socket to the remote endpoint. Catch any errors.    
        // Connect to Remote EndPoint  
        sender.Connect(remoteEP);

        Debug.Log(String.Format("Socket connected to {0}", sender.RemoteEndPoint.ToString()));

        while (sender.Connected)
        {
            // Receive the response from the remote device.    
            int bytesRec = sender.Receive(bytes);
            string data = Encoding.ASCII.GetString(Convert.FromBase64String(Encoding.ASCII.GetString(bytes)));
            MessageReceived(data);
            data = null;
        }

        // Release the socket.    
        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
    }

    public void SendMessageToServer(string _message)
    {
        Debug.Log(String.Format("<<<Client>>>: sent \"{0}\"", _message));
        byte[] messageBytes = Encoding.ASCII.GetBytes(Convert.ToBase64String(Encoding.ASCII.GetBytes(_message)) + "\0");
        sender.Send(messageBytes);
    }

    //listen for requests
    void messageReceivedListener(string _message)
    {
        Debug.Log(String.Format("<<<Client>>>: Recieved \"{0}\"", _message));
        try
        {
            //get the base request object
            MultiBaseRequest baseRequest = JsonUtility.FromJson<MultiBaseRequest>(_message);
            switch ((MultiPossibleRequest)baseRequest.RequestType)
            {
                //the server would like to spawn an object
                case MultiPossibleRequest.MultiSpawnObject:
                    MultiSpawnRequest multiSpawnRequest = JsonUtility.FromJson<MultiSpawnRequest>(baseRequest.Request);

                    //queue spawning the object
                    actions.Enqueue(() => SpawnObject(multiSpawnRequest.Index));
                    break;

                //the server would like to sync an object
                case MultiPossibleRequest.MultiSyncObject:
                    MultiSyncRequest multiSyncRequest = JsonUtility.FromJson<MultiSyncRequest>(baseRequest.Request);
                    //get the serialized transform object
                    SerializableTransform serializableTransform = JsonUtility.FromJson<SerializableTransform>(multiSyncRequest.transform);

                    //queue the transform copy
                    actions.Enqueue(() => serializableTransform.CopyToTransform(syncedObjects[multiSyncRequest.ID].transform));
                    break;

                //the server would like to change scenes
                case MultiPossibleRequest.MultiChangeScene:
                    MultiChangeSceneRequest multiChangeScene = JsonUtility.FromJson<MultiChangeSceneRequest>(baseRequest.Request);

                    //queue the scene change
                    actions.Enqueue(() => ChangeScene(multiChangeScene.Name));
                    currentScene = multiChangeScene.Name;
                    break;

                //the server would like to sync a scene object
                case MultiPossibleRequest.MultiSceneSyncObject:
                    MultiSceneSyncRequest multiSceneSync = JsonUtility.FromJson<MultiSceneSyncRequest>(baseRequest.Request);

                    //get the serializable transform
                    SerializableTransform aSerializableTransform = JsonUtility.FromJson<SerializableTransform>(multiSceneSync.transform);
                    //queue the transform copy
                    actions.Enqueue(() => aSerializableTransform.CopyToTransform(sceneSyncedObjects[multiSceneSync.ID].transform));
                    break;

                //the server wants us to take host authority
                case MultiPossibleRequest.MultiHostAuthChange:
                    hostAuthority = true;
                    break;

                //there's new connection to the game, send a dict of id's and indexes
                case MultiPossibleRequest.MultiNewConnection:
                    MultiNewConnection newConnection = JsonUtility.FromJson<MultiNewConnection>(baseRequest.Request);
                    //dictionary of the sceneobjects with non negative indices(root objects)
                    Dictionary<int, int> idIndexes = new Dictionary<int, int>();
                    foreach (int key in syncedObjects.Keys)
                    {
                        idIndexes.Add(key, syncedObjects[key].index);
                    }

                    //Send the request
                    MultiSceneObjects sceneObjects = new MultiSceneObjects(idIndexes, currentScene, syncedObjectsTotal, newConnection.threadN);
                    MultiBaseRequest request = new MultiBaseRequest(MultiPossibleRequest.MultiSceneObjects, JsonUtility.ToJson(sceneObjects));

                    break;

                //we've recieved a list of scene objects. deal with it
                case MultiPossibleRequest.MultiSceneObjects:
                    MultiSceneObjects multiSceneObjects = JsonUtility.FromJson<MultiSceneObjects>(baseRequest.Request);

                    //store the new synced objects total
                    syncedObjectsTotal = multiSceneObjects.syncedObjectsTotal;

                    //queue the scene change
                    actions.Enqueue(() => ChangeScene(multiSceneObjects.sceneName));
                    currentScene = multiSceneObjects.sceneName;

                    //queue spawning the objects
                    foreach (int key in multiSceneObjects.syncedObjects.Keys)
                    {
                        actions.Enqueue(() => SpawnObjectWithoutSetup(multiSceneObjects.syncedObjects[key], key));
                    }

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

    private IEnumerator SpawnNonVRPlayer()
    {
        //spawn the playerobject
        MultiSpawnRequest spawnRequest = new MultiSpawnRequest(0);
        MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSpawnObject, JsonUtility.ToJson(spawnRequest));
        string message = JsonUtility.ToJson(baseRequest);
        SendMessageToServer(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(1);
        Debug.Log("<<<Client>>>: Spawning Non-VR");
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
        MultiSpawnRequest spawnRequest = new MultiSpawnRequest(1);
        MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSpawnObject, JsonUtility.ToJson(spawnRequest));
        string message = JsonUtility.ToJson(baseRequest);
        SendMessageToServer(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(2);
        Debug.Log("<<<Client>>>: Spawning VR");
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
        sceneSyncedObjects = new SerializableDictionary<int, MultiSyncedObject>();
        foreach (GameObject g in sceneSyncedObjectList.sceneSyncedObjects)
        {
            Instantiate(g);
        }
    }
}
