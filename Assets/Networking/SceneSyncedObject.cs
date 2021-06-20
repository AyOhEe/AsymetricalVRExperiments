using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//very similar to a syncedobject, except from the fact that it always will belong to the server and must belong to a scene object
public class SceneSyncedObject : SyncedObject
{
    public enum LocalWhen
    {
        Client,
        Server,
        VR,
        NonVR
    }

    //we're local when the option selected is present
    public LocalWhen localWhenXPresent;

    //function pointer type to sendmessage on either client or server, whatever we're on
    public new delegate void SendMessageToOtherDelegate(string _message);
    public new event SendMessageToOtherDelegate SendMessageToOther;

    public bool isLocal()
    {
        bool retVal = false;
        switch (localWhenXPresent)
        {
            case LocalWhen.Client:
                retVal = client;
                break;
            case LocalWhen.Server:
                retVal = server;
                break;
            case LocalWhen.VR:
                retVal = GameObject.FindObjectOfType<VRPlayer>().GetComponent<SyncedObject>().localOwned;
                break;
            case LocalWhen.NonVR:
                retVal = GameObject.FindObjectOfType<NonVRPlayerController>().GetComponent<SyncedObject>().localOwned;
                break;
        }
        return retVal;
    }

    public new void Awake()
    {
        //do all the normal stuff for a synced object
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
            ID = client.sceneSyncedObjects.Count;
            client.sceneSyncedObjects.Add(client.sceneSyncedObjects.Count, this);
        }
        else
        {
            //yes, add our messageReceived listener
            server = gameServer.GetComponent<GameServer>();
            server.MessageReceived += MessageReceived;
            SendMessageToOther += server.SendMessageToClient;
            //also store our ID add ourselves to the list of synced objects
            ID = server.sceneSyncedObjects.Count;
            server.sceneSyncedObjects.Add(server.sceneSyncedObjects.Count, this);
        }

        //there are a lot of cases where we *might* be local
        localOwned = isLocal();

        //start syncing the object every syncInterval seconds
        Invoke("SyncObject", 0);
    }

    //sends a sync message
    public new void SyncObject()
    {
        //only sync to this instance of the object if it's locally owned
        if (localOwned)
        {
            //crete sync and base requests
            SceneSyncRequest syncRequest = new SceneSyncRequest(ID, transform);
            BaseRequest baseRequest = new BaseRequest(PossibleRequest.SceneSyncObject, JsonUtility.ToJson(syncRequest));
            //send the message away
            SendMessageToOther(JsonUtility.ToJson(baseRequest));
            Debug.Log("Scene Object " + ID.ToString() + " sent sync request");
        }

        //sync again after syncInterval seconds
        Invoke("SyncObject", syncInterval);
    }
}
