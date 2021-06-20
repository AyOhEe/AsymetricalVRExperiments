using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//request types
public enum MultiPossibleRequest
{
    MultiSpawnObject,
    MultiSyncObject,
    MultiChangeScene,
    MultiSceneSyncObject,
    MultiHostAuthChange,
    MultiNewConnection,
    MultiSceneObjects
}

//surface of mosts requests, contains type of request and request
[Serializable]
public struct MultiBaseRequest
{
    //type of request
    [SerializeField]
    public int RequestType;
    //request
    [SerializeField]
    public string Request;

    //construct a request
    public MultiBaseRequest(MultiPossibleRequest _requestType, string _request)
    {
        RequestType = (int)_requestType;
        Request = _request;
    }
}

//request to spawn an object
[Serializable]
public struct MultiSpawnRequest
{
    //index of object to spawn in spawnable objects
    [SerializeField]
    public int Index;

    //construct a new spawn request
    public MultiSpawnRequest(int _index)
    {
        Index = _index;
    }
}

//request to sync an object
[Serializable]
public struct MultiSyncRequest
{
    //id of object to sync
    [SerializeField]
    public int ID;
    //serialized transform
    [SerializeField]
    public string transform;

    //construct a sync request
    public MultiSyncRequest(int _id, Transform _transform)
    {
        ID = _id;
        transform = JsonUtility.ToJson(new SerializableTransform(_transform));
    }
}

//request to change scenes
[Serializable]
public struct MultiChangeSceneRequest
{
    //index of the scene to change to
    [SerializeField]
    public string Name;

    //construct a new scene change request
    public MultiChangeSceneRequest(string _name)
    {
        Name = _name;
    }
}

//request to sync a scene object
[Serializable]
public struct MultiSceneSyncRequest
{
    //id of object to sync
    [SerializeField]
    public int ID;
    //serialized transform
    [SerializeField]
    public string transform;

    //construct a scene sync request
    public MultiSceneSyncRequest(int _id, Transform _transform)
    {
        ID = _id;
        transform = JsonUtility.ToJson(new SerializableTransform(_transform));
    }
}

//sent from the server to the host indicating a new connection
[Serializable]
public struct MultiNewConnection
{
    //the connection requesting this
    [SerializeField]
    public int threadN;

    public MultiNewConnection(int _threadN)
    {
        threadN = _threadN;
    }
}

//sent from the host after recieving a MultiNewConnection request
[Serializable]
public struct MultiSceneObjects
{
    //dict of synced object ids with their prefab indexes
    [SerializeField]
    public Dictionary<int, int> syncedObjects;

    //current scene name
    [SerializeField]
    public string sceneName;

    //total number of scene objects
    [SerializeField]
    public int syncedObjectsTotal;

    //the thread that requested this
    [SerializeField]
    public int threadN;

    //construct a scene objects request
    public MultiSceneObjects(Dictionary<int, int> _syncedObjects, string _sceneName, int _syncedObjectsTotal, int _threadN)
    {
        syncedObjects = _syncedObjects;
        sceneName = _sceneName;
        syncedObjectsTotal = _syncedObjectsTotal;
        threadN = _threadN;
    }
}