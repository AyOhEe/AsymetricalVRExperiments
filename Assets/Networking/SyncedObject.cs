using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SyncedObject : MonoBehaviour
{
    //how often(in seconds) the object should sync
    public float syncInterval = 1.0f;
    //id
    public int ID;

    public GameClient client;
    public GameServer server;
    public bool localOwned;
    public GameObject parent;

    //function pointer type to sendmessage on either client or server, whatever we're on
    public delegate void SendMessageToOtherDelegate(string _message);
    public event SendMessageToOtherDelegate SendMessageToOther;

    private void Start()
    {
        if (parent)
        {
            localOwned = parent.GetComponent<SyncedObject>().localOwned;
        }
        //get the gameServer/Client
        GameObject gameClient = null, gameServer = GameObject.FindWithTag("GameServer");
        //does the server exist?
        if (gameServer == null)
        {
            //no, get the client as that *must* exist if there isn't a server and add our messageReceived listener
            gameClient = GameObject.FindWithTag("GameClient");
            client = gameClient.GetComponent<GameClient>();
            client.MessageReceived += MessageReceived;
            SendMessageToOther += client.SendMessageToServer;
            //also store our ID add ourselves to the list of synced objects
            ID = client.syncedObjects.Count;
            client.syncedObjects.Add(client.syncedObjects.Count, this);
        }
        else
        {
            //yes, add our messageReceived listener
            server = gameServer.GetComponent<GameServer>();
            server.MessageReceived += MessageReceived;
            SendMessageToOther += server.SendMessageToClient;
            //also store our ID add ourselves to the list of synced objects
            ID = server.syncedObjects.Count;
            server.syncedObjects.Add(server.syncedObjects.Count, this);
        }
        
        //start syncing the object every syncInterval seconds
        SyncObject();
    }

    //sends a sync message
    public void SyncObject()
    {
        //only sync to this instance of the object if it's locally owned
        if (localOwned)
        {
            //crete sync and base requests
            SyncRequest syncRequest = new SyncRequest(ID, transform);
            BaseRequest baseRequest = new BaseRequest(PossibleRequest.SyncObject, JsonUtility.ToJson(syncRequest));
            //send the message away
            SendMessageToOther(JsonUtility.ToJson(baseRequest));
            //sync again after syncInterval seconds
            Invoke("SyncObject", syncInterval);
        }
    }

    //called when a message is received on either the game client or game server, whichever is present
    public void MessageReceived(string _message)
    {

    }
}
