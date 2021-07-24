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
    MultiSceneObjects,
    MultiDespawnObject,
    GameManagerData
}

//surface of mosts requests, contains type of request and request
[Serializable]
public struct MultiBaseRequest
{
    //type of request
    [SerializeField]
    public int RT;
    //request
    [SerializeField]
    public string R;

    //construct a request
    public MultiBaseRequest(MultiPossibleRequest _requestType, string _request)
    {
        RT = (int)_requestType;
        R = _request;
    }
}

//request to spawn an object
[Serializable]
public struct MultiSpawnRequest
{
    //index of object to spawn in spawnable objects
    [SerializeField]
    public int I;

    //construct a new spawn request
    public MultiSpawnRequest(int _index)
    {
        I = _index;
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
    public List<SerializableTransform> tfs;

    //construct a sync request
    public MultiSyncRequest(int _id, Transform[] _transforms)
    {
        ID = _id;
        tfs = new List<SerializableTransform>();

        foreach (Transform T in _transforms)
        {
            tfs.Add(new SerializableTransform(T));
        }
    }
}

//request to change scenes
[Serializable]
public struct MultiChangeSceneRequest
{
    //index of the scene to change to
    [SerializeField]
    public string N;

    //construct a new scene change request
    public MultiChangeSceneRequest(string _name)
    {
        N = _name;
    }
}

//sent from the server to the host indicating a new connection
[Serializable]
public struct MultiNewConnection
{
    //the connection requesting this
    [SerializeField]
    public int tN;

    public MultiNewConnection(int _threadN)
    {
        tN = _threadN;
    }
}

//sent from the host after recieving a MultiNewConnection request
[Serializable]
public struct MultiSceneObjects
{
    //dict of synced object ids with their prefab indexes
    [SerializeField]
    public int[] sOI;
    [SerializeField]
    public int[] sOK;

    //current scene name
    [SerializeField]
    public string sN;

    //total number of scene objects
    [SerializeField]
    public int sOT;

    //the thread that requested this
    [SerializeField]
    public int tN;

    //construct a scene objects request
    public MultiSceneObjects(Dictionary<int, int> _syncedObjects, string _sceneName, int _syncedObjectsTotal, int _threadN)
    {
        sOK = new int[_syncedObjects.Count];
        sOI = new int[_syncedObjects.Count];
        _syncedObjects.Keys.CopyTo(sOK, 0);
        _syncedObjects.Values.CopyTo(sOI, 0);
        sN = _sceneName;
        sOT = _syncedObjectsTotal;
        tN = _threadN;
    }

    public Dictionary<int, int> syncedObjDict()
    {
        Dictionary<int, int> retVal = new Dictionary<int, int>();
        for (int i = 0; i < sOK.Length; i++)
        {
            retVal.Add(sOK[i], sOI[i]);
        }
        return retVal;
    }
}

//sent when an object is destroyed
[Serializable]
public struct MultiDespawnObject
{
    //ID of the object to be despawned
    [SerializeField]
    public int ID;

    public MultiDespawnObject(int _id)
    {
        ID = _id;
    }
}

//data to be passed among game managers
[Serializable]
public struct GameManagerData
{
    //data string
    [SerializeField]
    public string D;
    //data type, should be casted to an enum
    [SerializeField]
    public int T;

    public GameManagerData(string _D, int _T)
    {
        D = _D;
        T = _T;
    }
}