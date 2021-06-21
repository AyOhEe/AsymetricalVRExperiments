﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MultiSyncedObject : MonoBehaviour
{
    //how often(in seconds) the object should sync
    public float syncInterval = 1.0f;
    //do we do the synced object dict setup: MultiSyncedObject.cs:Awake
    public bool doSyncedObjectsDictSetup = true;
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

    public void Awake()
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
            client.syncedObjectsTotal++;
        }

        //start syncing the object every syncInterval seconds
        SyncObject();
    }

    public void Start()
    {
        //if the parent is locally owned, so are we
        if (parent)
        {
            localOwned = localOwned | parent.GetComponent<MultiSyncedObject>().localOwned;
        }
    }

    //sends a sync message
    public void SyncObject()
    {
        //only sync to this instance of the object if it's locally owned
        if (localOwned)
        {
            //crete sync and base requests
            MultiSyncRequest syncRequest = new MultiSyncRequest(ID, transform);
            MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSyncObject, JsonUtility.ToJson(syncRequest));
            //send the message away
            SendMessageToOther(JsonUtility.ToJson(baseRequest));
            Debug.Log("Object " + ID.ToString() + " sent sync request");
        }

        //sync again after syncInterval seconds
        Invoke("SyncObject", syncInterval);
    }

    //called when a message is received on either the game client or game server, whichever is present
    public void MessageReceived(string _message)
    {

    }
}