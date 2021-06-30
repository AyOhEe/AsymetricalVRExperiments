using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MultiSyncedObject : MonoBehaviour
{
    //how often(in seconds) the object should sync
    public float syncInterval = 1.0f;
    //do we do the synced object dict setup: MultiSyncedObject.cs:Awake
    public bool doSyncedObjectsDictSetup = true;

    //all of the transforms we need to sync
    public Transform[] syncedTransforms = new Transform[0];

    //id
    public int ID;
    //index of the prefab
    public int index = -1;

    public MultiClient client;
    //is the syncedObject owned locally?
    public bool localOwned;
    //parent object of this gameobject, if it exists.
    public GameObject parent;

    //function pointer type to sendmessage on either client or server, whatever we're on
    public delegate void SendMessageToOtherDelegate(string _message);
    public event SendMessageToOtherDelegate SendMessageToOther;

    public void Start()
    {
        //get the gameServer/Client
        GameObject gameClient = GameObject.FindWithTag("GameClient");

        //get the client and setup events
        client = gameClient.GetComponent<MultiClient>();
        SendMessageToOther += client.SendMessageToServer;
        client.MessageReceived += MessageReceived;


        if (doSyncedObjectsDictSetup)
        {
            ID = client.syncedObjectsTotal;
            client.syncedObjects.Add(client.syncedObjectsTotal, this);
            //increment the total synced objects count
            client.syncedObjectsTotal += 1;
        }


        //if the parent is locally owned, so are we
        if (parent)
        {
            localOwned = localOwned | parent.GetComponent<MultiSyncedObject>().localOwned;
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
            //create sync and base requests
            MultiSyncRequest syncRequest = new MultiSyncRequest(ID, syncedTransforms);
            MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSyncObject, JsonUtility.ToJson(syncRequest));
            //send the message away
            SendMessageToOther(JsonUtility.ToJson(baseRequest));
            Debug.Log("Object " + ID.ToString() + " sent sync request");
        }

        //sync again after syncInterval seconds
        Invoke("SyncObject", syncInterval);
    }

    //syncs all of the synced transforms in transforms to the request
    public void HandleSyncRequest(MultiSyncRequest request)
    {
        //iterate through the transforms in the request
        for (int i = 0; i < request.tfs.Count; i++)
        {
            //copy the transforms from the request to the objects
            request.tfs[i].CopyToTransform(syncedTransforms[i]);
        }
    }

    //called when a message is received on either the game client or game server, whichever is present
    public void MessageReceived(string _message)
    {

    }

    public void OnDestroy()
    {
        Debug.Log(String.Format("{0} Destroyed!", gameObject.name));
        //if we're locally owned, send a destroy object request
        if (localOwned)
        {
            MultiDespawnObject destroyObject = new MultiDespawnObject(ID);
            MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiDespawnObject, JsonUtility.ToJson(destroyObject));
            SendMessageToOther(JsonUtility.ToJson(baseRequest));
        }
    }
}
