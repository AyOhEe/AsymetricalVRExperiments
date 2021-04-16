using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncedObject : MonoBehaviour
{
    //how often(in seconds) the object should sync
    public float syncInterval;
    //id
    public int ID;

    GameClient client;
    GameServer server;
    bool serverObject;

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
            serverObject = false;
            gameClient = GameObject.FindWithTag("GameClient");
            client = gameClient.GetComponent<GameClient>();
            client.MessageReceived += MessageReceived;
            SendMessageToOther += client.SendMessageToServer;
            //also add ourselves to the list of synced objects
            client.syncedObjects.Add(this);
        }
        else
        {
            //yes, add our messageReceived listener
            server = gameServer.GetComponent<GameServer>();
            server.MessageReceived += MessageReceived;
            SendMessageToOther += server.SendMessageToClient;
            //also add ourselves to the list of synced objects
            server.syncedObjects.Add(this);
        }
        
        SyncObject();
    }

    //sends a sync message
    public void SyncObject()
    {
        string message = "";
        message += PossibleRequest.SyncObject.ToString() + '"';
        message += '"' + ID.ToString() + '"';
        message += '"' + (transform.position + new Vector3(0, 1, 0)).ToString() + '"';
        message += '"' + transform.rotation.ToString();
        SendMessageToOther(message);
    }

    //called when a message is received on either the game client or game server, whichever is present
    public void MessageReceived(string _message)
    {

    }
}
