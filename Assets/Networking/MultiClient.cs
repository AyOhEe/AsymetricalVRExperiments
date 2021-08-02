using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using MessagePack;

public enum InputMethod
{
    VR,
    NonVR
}

public class MultiClient : MonoBehaviour
{
    //ip to connect to
    public string ConnectionIP;
    //the socket connected to the server
    Socket sender;

    //delegate type for message received
    public delegate void MessageReceiveEvent(byte[] _messsage);
    //called when a message is received;
    public event MessageReceiveEvent MessageReceived;

    //dicts of all synced objects
    public Dictionary<int, MultiSyncedObject> syncedObjects = new Dictionary<int, MultiSyncedObject>();
    public Dictionary<int, MultiSyncedObject> sceneSyncedObjects = new Dictionary<int, MultiSyncedObject>();
    public int syncedObjectsTotal = 0;
    //list of all spawnable objects
    public List<GameObject> spawnableObjects;
    //list of all scenes possible to switch to
    public List<SceneInfo> possibleScenes;

    //queue of actions to be done
    public Queue<Action> actions = new Queue<Action>();

    //input method the client is using
    public InputMethod inputMethod;

    public bool hostAuthority;
    public SceneInfo currentScene;

    private GameObject lastSOSpawnedWOSetup;
    private int spawnsWithoutInstantiate;

    //queue of objects that need to be locally spawned
    private Queue<Action> localSpawnActions = new Queue<Action>();

    //all of the game services in this scene;
    public Dictionary<int, GameSystem> gameSystems = new Dictionary<int, GameSystem>();
    
    //all of the players in this scene, keyed by their clientID
    public Dictionary<int, GamePlayer> gamePlayers = new Dictionary<int, GamePlayer>();
    public GamePlayer[] gamePlayersList = new GamePlayer[10];

    //the client id of this client
    public int _ClientID;

    // Update is called once per frame
    void Update()
    {
        //run all actions queued
        if (actions.Count != 0)
        {
            lock (actions)
            {
                while (actions.Count != 0)
                {
                    try
                    {
                        actions.Dequeue().Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }
        }

        //this only exists to see it in the editor
#if UNITY_EDITOR
        gamePlayers.Values.CopyTo(gamePlayersList, 0);
#endif
    }

    //spawns an object in spawnable objects from an id
    public GameObject SpawnObject(int index)
    {
        //spawn in the new synced object instance
        GameObject instance = Instantiate(spawnableObjects[index]);
        MultiSyncedObject syncedObject = instance.GetComponent<MultiSyncedObject>();
        //store it's index
        syncedObject.index = index;
        return instance;
    }
    //spawns an object in spawnable objects from an id
    public GameObject SpawnObjectWithoutSetup(int index, int ID)
    {
        //it's not a child, spawn in a new instance of the synced object
        GameObject instance = Instantiate(spawnableObjects[index]);
        MultiSyncedObject syncedObject = instance.GetComponent<MultiSyncedObject>();
        //it shouldn't do dict setup
        syncedObject.doSyncedObjectsDictSetup = false;
        //store index and id
        syncedObject.index = index;
        syncedObject.ID = ID;
        //this is the last synced object spawned by this method
        lastSOSpawnedWOSetup = instance;

        //if this object has children, make sure they don't do dict setup
        if (instance.GetComponentsInChildren<MultiSyncedObject>().Length > 0)
        {
            foreach (MultiSyncedObject m in instance.GetComponentsInChildren<MultiSyncedObject>())
            {
                m.doSyncedObjectsDictSetup = false;
            }
        }

        syncedObjects.Add(ID, syncedObject.GetComponent<MultiSyncedObject>());

        return instance;
    }

    //spawns a local object
    public GameObject LocalSpawnObject(int index)
    {
        //spawn in the new synced object instance
        GameObject instance = Instantiate(spawnableObjects[index]);
        MultiSyncedObject syncedObject = instance.GetComponent<MultiSyncedObject>();
        //store its index and its local status
        syncedObject.localOwned = true;
        syncedObject.index = index;

        //send a request to all other clients to spawn in this object
        MultiSpawnRequest spawnRequest = new MultiSpawnRequest(index);
        MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSpawnObject, MessagePackSerializer.Serialize(spawnRequest));
        SendMessageToServer(MessagePackSerializer.Serialize(baseRequest));
        return instance;
    }

    /// <summary> 	
    /// Setup socket connection. 	
    /// </summary> 	
    public void ConnectToTcpServer()
    {
        try
        {
            //create the background thread to listen to the tcp server
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
        //we need to make sure the listener delegate isn't null
        if (MessageReceived == null)
        {
            MessageReceived += messageReceivedListener;
        }

        // Connect to a Remote server  
        // Get Host IP Address that is used to establish a connection   
        IPAddress ipAddress = IPAddress.Parse(ConnectionIP);
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, 25565);

        // Create a TCP/IP  socket.    
        sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Connect the socket to the remote endpoint. Catch any errors.    
        // Connect to Remote EndPoint  
        sender.Connect(remoteEP);

        Debug.Log(String.Format("Socket connected to {0}", sender.RemoteEndPoint.ToString()));
        try
        {
            while (sender.Connected)
            {
                // Receive the response from the remote device.    
                byte[] bytes = new byte[2048];
                int bytesRec = sender.Receive(bytes);
                MessageReceived(bytes);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }

        // Release the socket.    
        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
    }

    public void SendMessageToServer(byte[] _message)
    {
        //only send a message if we're connected
        if (sender.Connected)
        {
            //send the message, we're connected
            Debug.Log(String.Format("<<<Client>>>: sent \"{0}\"", MessagePackSerializer.ConvertToJson(_message)));
            sender.Send(_message);
        }
    }

    //listen for requests
    void messageReceivedListener(byte[] _message)
    {
        try
        {
            //get the base request object
            MultiBaseRequest baseRequest = MessagePackSerializer.Deserialize<MultiBaseRequest>(_message);
            Debug.Log(String.Format("<<<Client>>>: Recieved \"{0}\"", MessagePackSerializer.ConvertToJson(_message)));
            switch ((MultiPossibleRequest)baseRequest.RT)
            {
                //initial data for setup
                case MultiPossibleRequest.MultiInitialData:
                    MultiInitialData initialData = MessagePackSerializer.Deserialize<MultiInitialData>(baseRequest.R);

                    //store the thread key as the client id, they're the same thing
                    _ClientID = initialData.T;
                    actions.Enqueue(() => StartCoroutine(HandleMultiInitialData(initialData)));
                    break;

                //the server would like to spawn an object
                case MultiPossibleRequest.MultiSpawnObject:
                    MultiSpawnRequest multiSpawnRequest = MessagePackSerializer.Deserialize<MultiSpawnRequest>(baseRequest.R);

                    //queue spawning the object
                    actions.Enqueue(() => SpawnObject(multiSpawnRequest.I));
                    break;

                //the server would like to sync an object
                case MultiPossibleRequest.MultiSyncObject:
                    MultiSyncRequest multiSyncRequest = MessagePackSerializer.Deserialize<MultiSyncRequest>(baseRequest.R);

                    //queue the synced object handle
                    actions.Enqueue(() => syncedObjects[multiSyncRequest.ID].HandleSyncRequest(multiSyncRequest));
                    break;

                //the server would like to change scenes
                case MultiPossibleRequest.MultiChangeScene:
                    MultiChangeSceneRequest multiChangeScene = MessagePackSerializer.Deserialize<MultiChangeSceneRequest>(baseRequest.R);

                    //queue the scene change
                    actions.Enqueue(() => SceneManager.LoadScene(possibleScenes[multiChangeScene.N].name));
                    currentScene = possibleScenes[multiChangeScene.N];
                    
                    actions.Enqueue(() =>
                    {
                        //get all of the systems in this scene
                        GameSystem[] systems = FindObjectsOfType<GameSystem>();
                        //iterate through them, storing a reference to each of them
                        foreach (GameSystem system in systems)
                        {
                            gameSystems.Add(system.SystemID, system);
                        }
                    });

                    //reset the dictionary of players
                    gamePlayers = new Dictionary<int, GamePlayer>();

                    //spawn our prefab on the network
                    MultiSpawnPlayer spawnPlayerScene = new MultiSpawnPlayer((int)inputMethod, _ClientID, FindObjectOfType<TeamSystem>().localTeam);
                    MultiBaseRequest spawnPlayerBaseScene = new MultiBaseRequest(MultiPossibleRequest.MultiSpawnPlayer, MessagePackSerializer.Serialize(spawnPlayerScene));
                    SendMessageToServer(MessagePackSerializer.Serialize(spawnPlayerBaseScene));

                    //spawn our prefab here
                    actions.Enqueue(() =>
                    {
                        //spawn in the new synced object instance
                        GameObject instance = Instantiate(possibleScenes[multiChangeScene.N].PlayerPrefabs[(int)inputMethod]);
                        GamePlayer player = instance.GetComponent<GamePlayer>();
                        //store its index and its local status
                        player.LocalOwned = true;

                        //do setup
                        player.PlayerSetup(_ClientID, FindObjectOfType<TeamSystem>().localTeam);
                        //store it
                        gamePlayers[_ClientID] = player;
                    });
                    break;

                //the server would like to sync a scene object
                case MultiPossibleRequest.MultiSceneSyncObject:
                    MultiSyncRequest multiSceneSync = MessagePackSerializer.Deserialize<MultiSyncRequest>(baseRequest.R);
                    
                    //queue the synced object handle
                    actions.Enqueue(() => sceneSyncedObjects[multiSceneSync.ID].HandleSyncRequest(multiSceneSync));
                    break;

                //the server wants us to take host authority
                case MultiPossibleRequest.MultiHostAuthChange:
                    hostAuthority = true;
                    break;

                //there's new connection to the game, send a dict of id's and indexes
                case MultiPossibleRequest.MultiNewConnection:
                    MultiNewConnection newConnection = MessagePackSerializer.Deserialize<MultiNewConnection>(baseRequest.R);
                    //dictionary of the sceneobjects with non negative indices(root objects)
                    Dictionary<int, int> idIndexes = new Dictionary<int, int>();
                    foreach (int key in syncedObjects.Keys)
                    {
                        idIndexes.Add(key, syncedObjects[key].index);
                    }
                    

                    //Send the request
                    MultiInitialData sceneObjects = new MultiInitialData(newConnection.tN, idIndexes, gamePlayers, possibleScenes.IndexOf(currentScene), syncedObjectsTotal, newConnection.tN);
                    MultiBaseRequest request = new MultiBaseRequest(MultiPossibleRequest.MultiInitialData, MessagePackSerializer.Serialize(sceneObjects));
                    SendMessageToServer(MessagePackSerializer.Serialize(request));
                    
                    break;

                //the server would like us to despawn an object
                case MultiPossibleRequest.MultiDespawnObject:
                    MultiDespawnObject despawnObject = MessagePackSerializer.Deserialize<MultiDespawnObject>(baseRequest.R);

                    //destroy the object
                    Debug.Log(String.Format("MultiClient.cs:291 Destroying {0}", syncedObjects[despawnObject.ID].gameObject.name));
                    actions.Enqueue(() => Destroy(syncedObjects[despawnObject.ID].gameObject));
                    break;

                case MultiPossibleRequest.MultiGameData:
                    GameSystemData systemData = MessagePackSerializer.Deserialize<GameSystemData>(baseRequest.R);

                    //pass on this data, the game system knows what to do with it
                    gameSystems[systemData.S].HandleMessage(systemData);
                    break;

                case MultiPossibleRequest.MultiSpawnPlayer:
                    MultiSpawnPlayer spawnPlayer = MessagePackSerializer.Deserialize<MultiSpawnPlayer>(baseRequest.R);

                    //spawn in the player and ensure it knows which client it belongs to
                    actions.Enqueue(() =>
                    {
                        GameObject instance = Instantiate(currentScene.PlayerPrefabs[spawnPlayer.T]);

                        GamePlayer gamePlayer = instance.GetComponent<GamePlayer>();
                        //do setup
                        gamePlayer.PlayerSetup(spawnPlayer.C, (TeamSystem.Team)spawnPlayer.t);

                        //store it
                        gamePlayers[spawnPlayer.C] = gamePlayer;
                    });
                    break;
                case MultiPossibleRequest.MultiSyncPlayer:
                    MultiSyncPlayer syncPlayer = MessagePackSerializer.Deserialize<MultiSyncPlayer>(baseRequest.R);

                    actions.Enqueue(() => gamePlayers[syncPlayer.C].HandleMessage(syncPlayer.S));

                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    private void Start()
    {
        //disable vr
        UnityEngine.XR.XRSettings.enabled = false;
        //add our message received listener to the message received event
        MessageReceived += messageReceivedListener;

    }

    private IEnumerator HandleMultiInitialData(MultiInitialData multiSceneObjects)
    {
        //clear the old synced objects dict and destroy the old synced objects
        foreach (int key in syncedObjects.Keys)
        {
            Debug.Log(String.Format("MultiClient.cs:269 Destroying {0}", syncedObjects[key].gameObject.name));
            Destroy(syncedObjects[key].gameObject);
            syncedObjects.Remove(key);
        }

        //queue the scene change(maybe)
        SceneManager.LoadScene(possibleScenes[multiSceneObjects.sI].name);
        currentScene = possibleScenes[multiSceneObjects.sI];

        yield return new WaitForSeconds(0.1f);

        //queue spawning the new objects
        Dictionary<int, int> newSyncedObjects = multiSceneObjects.syncedObjDict();
        foreach (int key in newSyncedObjects.Keys)
        {
            SpawnObjectWithoutSetup(newSyncedObjects[key], key);
        }

        //reset the synced objects total
        syncedObjectsTotal = newSyncedObjects.Count;

        actions.Enqueue(() =>
        {
            //get all of the systems in this scene
            GameSystem[] systems = FindObjectsOfType<GameSystem>();
            //iterate through them, storing a reference to each of them
            foreach (GameSystem system in systems)
            {
                gameSystems.Add(system.SystemID, system);
            }
        });

        //spawn our prefab on the network
        MultiSpawnPlayer spawnPlayerScene = new MultiSpawnPlayer((int)inputMethod, _ClientID, FindObjectOfType<TeamSystem>().localTeam);
        MultiBaseRequest spawnPlayerBaseScene = new MultiBaseRequest(MultiPossibleRequest.MultiSpawnPlayer, MessagePackSerializer.Serialize(spawnPlayerScene));
        SendMessageToServer(MessagePackSerializer.Serialize(spawnPlayerBaseScene));

        //spawn our prefab here
        actions.Enqueue(() =>
        {
            //spawn in the new synced object instance
            GameObject instance = Instantiate(possibleScenes[multiSceneObjects.sI].PlayerPrefabs[(int)inputMethod]);
            GamePlayer player = instance.GetComponent<GamePlayer>();
            //store its index and its local status
            player.LocalOwned = true;

            //do setup
            player.PlayerSetup(_ClientID, FindObjectOfType<TeamSystem>().localTeam);

            //store it
            gamePlayers[_ClientID] = player;
        });

        //spawn the rest of the players in
        Dictionary<int, Tuple<int,int>> newGamePlayers = multiSceneObjects.gamePlayerDict();
        foreach (int key in newGamePlayers.Keys)
        {
            actions.Enqueue(() => 
            {
                //spawn in the new synced object instance
                GameObject instance = Instantiate(possibleScenes[multiSceneObjects.sI].PlayerPrefabs[newGamePlayers[key].Item1]);
                GamePlayer newPlayer = instance.GetComponent<GamePlayer>();

                //do setup
                newPlayer.PlayerSetup(key, (TeamSystem.Team)newGamePlayers[key].Item2);

                //store it
                gamePlayers[key] = newPlayer;
            });
        }
    }

    public IEnumerator ChangeScene(int sceneIndex, bool forceSceneChange = false)
    {
        //only order a scene change if we're host or it is forced(basically only in MultiNetworkHandler.cs:75)
        if (hostAuthority | forceSceneChange)
        {
            //change the scene
            SceneManager.LoadScene(possibleScenes[sceneIndex].name);
            currentScene = possibleScenes[sceneIndex];

            //send a change scene request
            MultiChangeSceneRequest sceneRequest = new MultiChangeSceneRequest(sceneIndex);
            MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiChangeScene, MessagePackSerializer.Serialize(sceneRequest));
            SendMessageToServer(MessagePackSerializer.Serialize(baseRequest));

            yield return new WaitForEndOfFrame();

            //spawn our prefab on the network
            MultiSpawnPlayer spawnPlayer = new MultiSpawnPlayer((int)inputMethod, _ClientID, FindObjectOfType<TeamSystem>().localTeam);
            MultiBaseRequest spawnPlayerBase = new MultiBaseRequest(MultiPossibleRequest.MultiSpawnPlayer, MessagePackSerializer.Serialize(spawnPlayer));
            SendMessageToServer(MessagePackSerializer.Serialize(spawnPlayerBase));

            actions.Enqueue(() =>
            {
                //spawn in the new synced object instance
                GameObject instance = Instantiate(possibleScenes[sceneIndex].PlayerPrefabs[(int)inputMethod]);
                GamePlayer player = instance.GetComponent<GamePlayer>();

                //do setup
                player.PlayerSetup(_ClientID, FindObjectOfType<TeamSystem>().localTeam);

                //store it
                gamePlayers[_ClientID] = player;
            });
        }
    }
}
