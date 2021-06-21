﻿using System;
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
    private TcpClient connectedTcpClient;

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
    public SerializableDictionary<int, SyncedObject> syncedObjects = new SerializableDictionary<int, SyncedObject>();
    public SerializableDictionary<int, SyncedObject> sceneSyncedObjects = new SerializableDictionary<int, SyncedObject>();

    //list of all spawnable objects
    public List<GameObject> spawnableObjects;

    //list of all scenes possible to switch to
    public List<Scene> possibleScenes;

    //queue of actions to be done
    private Queue<Action> actions = new Queue<Action>();

    //input method the server is using
    public InputMethod inputMethod;

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

    // Use this for initialization
    public void StartServer()
    {
        // Start TcpServer background thread 		
        Thread tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
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
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 25565);
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
                            actions.Enqueue(() => SceneManager.LoadScene("ExperimentSelector"));
                            actions.Enqueue(() => StartCoroutine(ChangeScene("ExperimentSelector")));

                            // Read incomming stream into byte arrary. 	
                            string[] messages;
                            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var incommingData = new byte[length];
                                Array.Copy(bytes, 0, incommingData, 0, length);

                                clientMessage = Encoding.ASCII.GetString(incommingData);

                                //get all messages in the string
                                messages = clientMessage.Split(new string[] { "\0" }, StringSplitOptions.RemoveEmptyEntries);

                                for(int i = 0; i < messages.Length; i++)
                                {
                                    try
                                    {
                                        // Convert byte array to string message. 					
                                        messages[i] = Encoding.ASCII.GetString(Convert.FromBase64String(messages[i]));
                                        //call the message received event
                                        MessageReceived(messages[i]);
                                    }
                                    catch (FormatException formatExcept)
                                    {
                                        Debug.Log(messages[i]);
                                        Debug.Log(formatExcept);
                                    }
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
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(_message + "\0");
                //encode into base 64
                string serverMessageAsB64String = Convert.ToBase64String(serverMessageAsByteArray);
                byte[] serverMessageAsB64Bytes = Encoding.ASCII.GetBytes(serverMessageAsB64String + "\0");
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsB64Bytes, 0, serverMessageAsB64Bytes.Length);
                Debug.Log("Server Sent: '" + _message + "\0" + "'");
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

                //the client would like to sync a scene object
                case "SceneSyncObject":
                    //get the scene sync object
                    SceneSyncRequest sceneSyncRequest = JsonUtility.FromJson<SceneSyncRequest>(baseRequest.Request);
                    //get the serializable transform
                    SerializableTransform aSerializableTransform = JsonUtility.FromJson<SerializableTransform>(sceneSyncRequest.transform);
                    //sync the object
                    actions.Enqueue(() => aSerializableTransform.CopyToTransform(sceneSyncedObjects[sceneSyncRequest.ID].transform));
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
        SendMessageToClient(message);
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
        SendMessageToClient(message);
        //wait and get the object after it's probably spawned
        yield return new WaitForSeconds(2);
        Debug.Log("Spawning VR");
        GameObject vr = GameObject.FindGameObjectWithTag("VRHead");
    }

    public IEnumerator ChangeScene(string name)
    {
        //ask the client to change scenes
        ChangeSceneRequest changeSceneRequest = new ChangeSceneRequest(name);
        BaseRequest baseRequest = new BaseRequest(PossibleRequest.ChangeScene, JsonUtility.ToJson(changeSceneRequest));
        string message = JsonUtility.ToJson(baseRequest);
        SendMessageToClient(message);

        //wait for two frames or so
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        //get all of the synced objects that should be in the scene and spawn them in
        SceneSyncedObjectList sceneSyncedObjectList = GameObject.FindObjectOfType<SceneSyncedObjectList>();
        sceneSyncedObjects = new SerializableDictionary<int, SyncedObject>();
        foreach(GameObject g in sceneSyncedObjectList.sceneSyncedObjects)
        {
            Instantiate(g);
        }
    }
}