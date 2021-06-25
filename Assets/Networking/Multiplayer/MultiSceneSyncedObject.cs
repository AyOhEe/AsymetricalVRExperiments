using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//very similar to a syncedobject, except from the fact that it always will belong to the server and must belong to a scene object
public class MultiSceneSyncedObject : MultiSyncedObject
{
    //function pointer type to sendmessage on either client or server, whatever we're on
    public new delegate void SendMessageToOtherDelegate(string _message);
    public new event SendMessageToOtherDelegate SendMessageToOther;

    private new void Awake()
    {
        //get the gameServer/Client
        GameObject gameClient = GameObject.FindWithTag("GameClient");

        //get the client and setup events
        client = gameClient.GetComponent<MultiClient>();

        doSyncedObjectsDictSetup = false;

        localOwned = client.hostAuthority;
        client.MessageReceived += MessageReceived;
        client.sceneSyncedObjects.Add(ID, this);

        Invoke("SyncObject", syncInterval);
    }

    //sends a sync message
    public new void SyncObject()
    {
        localOwned = client.hostAuthority;

        //only sync to this instance of the object if it's locally owned
        if (localOwned)
        {
            //crete sync and base requests
            MultiSyncRequest syncRequest = new MultiSyncRequest(ID, syncedTransforms);
            MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSceneSyncObject, JsonUtility.ToJson(syncRequest));
            //send the message away
            SendMessageToOther(JsonUtility.ToJson(baseRequest));
            Debug.Log("Scene Object " + ID.ToString() + " sent sync request");
        }

        //sync again after syncInterval seconds
        Invoke("SyncObject", syncInterval);
    }

    //called when a message is received on either the game client or game server, whichever is present
    public new void MessageReceived(string _message)
    {
        //check if it's a host auth change, otherwise we don't care
        if ((MultiPossibleRequest)JsonUtility.FromJson<MultiBaseRequest>(_message).RequestType == MultiPossibleRequest.MultiHostAuthChange)
        {
            //it was, we're locally owned now
            localOwned = true;
        }
    }
}
