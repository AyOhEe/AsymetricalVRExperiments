using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//request types
public enum MultiPossibleRequest
{
    MultiInitialData,
    MultiSpawnObject,
    MultiSyncObject,
    MultiChangeScene,
    MultiSceneSyncObject,
    MultiHostAuthChange,
    MultiNewConnection,
    MultiSceneObjects,
    MultiDespawnObject,
    GameSystemData,
    MultiSpawnPlayer,
    MultiSyncPlayer
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
    public int N;

    //construct a new scene change request
    public MultiChangeSceneRequest(int _index)
    {
        N = _index;
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

    //dict of gameplayer types with their client ids
    [SerializeField]
    public int[] gOC;
    [SerializeField]
    public int[] gOT;

    //current scene index
    [SerializeField]
    public int sI;

    //total number of scene objects
    [SerializeField]
    public int sOT;

    //the thread that requested this
    [SerializeField]
    public int tN;

    //construct a scene objects request
    public MultiSceneObjects(Dictionary<int, int> _syncedObjects, Dictionary<int, int> _gamePlayers, int _sceneIndex, int _syncedObjectsTotal, int _threadN)
    {
        sOK = new int[_syncedObjects.Count];
        sOI = new int[_syncedObjects.Count];
        _syncedObjects.Keys.CopyTo(sOK, 0);
        _syncedObjects.Values.CopyTo(sOI, 0);
        gOC = new int[_gamePlayers.Count];
        gOT = new int[_gamePlayers.Count];
        _gamePlayers.Keys.CopyTo(gOC, 0);
        _gamePlayers.Values.CopyTo(gOT, 0);
        sI = _sceneIndex;
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

    public Dictionary<int, int> gamePlayerDict()
    {
        Dictionary<int, int> retVal = new Dictionary<int, int>();
        for (int i = 0; i < gOC.Length; i++)
        {
            retVal.Add(gOC[i], gOT[i]);
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

//initial data sent to help keep track of clients
[Serializable]
public struct MultiInitialData
{
    //thread key
    [SerializeField]
    public int T;

    public MultiInitialData(int _T)
    {
        T = _T;
    }
}

//data sent between game systems
[Serializable]
public struct GameSystemData
{
    //service id
    [SerializeField]
    public int S;

    //data string
    [SerializeField]
    public string D;

    public GameSystemData(int _S, string _D)
    {
        S = _S;
        D = _D;
    }
}

//request to spawn player
[Serializable]
public struct MultiSpawnPlayer
{
    //player type
    [SerializeField]
    public int T;

    //client id
    [SerializeField]
    public int C;

    public MultiSpawnPlayer(int _T, int _C)
    {
        T = _T;
        C = _C;
    }
}

//request to sync player
[Serializable]
public struct MultiSyncPlayer
{
    //client id
    [SerializeField]
    public int C;

    //sync string
    [SerializeField]
    public string S;

    public MultiSyncPlayer(int _C, string _S)
    {
        C = _C;
        S = _S;
    }
}