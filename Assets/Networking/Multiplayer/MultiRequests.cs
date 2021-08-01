using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MessagePack;

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

    MultiDespawnObject,

    MultiGameData,

    MultiSpawnPlayer,
    MultiSyncPlayer,
}

//surface of mosts requests, contains type of request and request
[MessagePackObject]
public struct MultiBaseRequest
{
    //type of request
    [Key(0)]
    public int RT;
    //request
    [Key(1)]
    public byte[] R;

    //construct a request
    public MultiBaseRequest(MultiPossibleRequest _requestType, byte[] _request)
    {
        RT = (int)_requestType;
        R = _request;
    }
}

//request to spawn an object
[MessagePackObject]
public struct MultiSpawnRequest
{
    //index of object to spawn in spawnable objects
    [Key(0)]
    public int I;

    //construct a new spawn request
    public MultiSpawnRequest(int _index)
    {
        I = _index;
    }
}

//request to sync an object
[MessagePackObject]
public struct MultiSyncRequest
{
    //id of object to sync
    [Key(0)]
    public int ID;
    //serialized transform
    [Key(1)]
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
[MessagePackObject]
public struct MultiChangeSceneRequest
{
    //index of the scene to change to
    [Key(0)]
    public int N;

    //construct a new scene change request
    public MultiChangeSceneRequest(int _index)
    {
        N = _index;
    }
}

//sent from the server to the host indicating a new connection
[MessagePackObject]
public struct MultiNewConnection
{
    //the connection requesting this
    [Key(0)]
    public int tN;

    public MultiNewConnection(int _threadN)
    {
        tN = _threadN;
    }
}

//sent when an object is destroyed
[MessagePackObject]
public struct MultiDespawnObject
{
    //ID of the object to be despawned
    [Key(0)]
    public int ID;

    public MultiDespawnObject(int _id)
    {
        ID = _id;
    }
}

//initial data sent to help keep track of clients
[MessagePackObject]
public struct MultiInitialData
{
    //thread key
    [Key(0)]
    public int T;

    //dict of synced object ids with their prefab indexes
    [Key(1)]
    public int[] sOI;
    [Key(2)]
    public int[] sOK;

    //dict of gameplayer types with their client ids
    [Key(3)]
    public int[] gOC;
    [Key(4)]
    public int[] gOT;

    //current scene index
    [Key(5)]
    public int sI;

    //total number of scene objects
    [Key(6)]
    public int sOT;

    //the thread that requested this
    [Key(7)]
    public int tN;

    public MultiInitialData(int _T, Dictionary<int, int> _syncedObjects, Dictionary<int, int> _gamePlayers, int _sceneIndex, int _syncedObjectsTotal, int _threadN)
    {
        T = _T;

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

//data sent between game systems
[MessagePackObject]
public struct GameSystemData
{
    //service id
    [Key(0)]
    public int S;

    //data string
    [Key(1)]
    public byte[] D;

    public GameSystemData(int _S, byte[] _D)
    {
        S = _S;
        D = _D;
    }
}

//request to spawn player
[MessagePackObject]
public struct MultiSpawnPlayer
{
    //player type
    [Key(0)]
    public int T;

    //client id
    [Key(1)]
    public int C;

    public MultiSpawnPlayer(int _T, int _C)
    {
        T = _T;
        C = _C;
    }
}

//request to sync player
[MessagePackObject]
public struct MultiSyncPlayer
{
    //client id
    [Key(0)]
    public int C;

    //sync string
    [Key(1)]
    public byte[] S;

    public MultiSyncPlayer(int _C, byte[] _S)
    {
        C = _C;
        S = _S;
    }
}