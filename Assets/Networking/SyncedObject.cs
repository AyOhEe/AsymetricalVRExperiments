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

    //function pointer type to sendmessage on either client or server, whatever we're on
    public delegate void SendMessageToOtherDelegate(string _message);
    public event SendMessageToOtherDelegate SendMessageToOther;

    private void Start()
    {
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
        
        SyncObject();
    }

    //sends a sync message
    public void SyncObject()
    {
        if (localOwned)
        {
            string message = "";
            message += PossibleRequest.SyncObject.ToString() + '"';
            message += '"' + ID.ToString() + '"';
            message += '"' + transform.position.ToString() + '"';
            message += '"' + transform.rotation.ToString();
            SendMessageToOther(message);
            Invoke("SyncObject", syncInterval);
        }
    }

    //called when a message is received on either the game client or game server, whichever is present
    public void MessageReceived(string _message)
    {

    }
}
