using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//request types
public enum PossibleRequest
{
    ConnectionRequest,
    DisconnectRequest,
    SyncObject,
    SpawnObject,
    ChangeScene
}

//surface of mosts requests, contains type of request and request
[Serializable]
public struct BaseRequest
{
    //type of request
    [SerializeField]
    public string RequestType;
    //request
    [SerializeField]
    public string Request;

    //construct a request
    public BaseRequest(PossibleRequest _requestType, string _request)
    {
        RequestType = _requestType.ToString();
        Request = _request;
    }
}
//only used when connecting, ensures there's only one of each inputMethod in a game
[Serializable]
public struct ConnectionRequest
{
    //input method requested
    [SerializeField]
    public InputMethod requestedInput;

    //construct a connection request
    public ConnectionRequest(InputMethod _requestedInput)
    {
        requestedInput = _requestedInput;
    }
}
//only used when shutting down or denying connection
[Serializable]
public struct DisconnectRequest
{
    //possible reasons to disconnect
    public enum DisconnectReason
    {
        InputMethodPresent,
        ShuttingDown
    }

    //the reason to disconnect
    public DisconnectReason disconnectReason;
    
    //construct a disconnect request
    public DisconnectRequest(DisconnectReason _disconnectReason)
    {
        disconnectReason = _disconnectReason;
    }
}
//request to sync an object
[Serializable]
public struct SyncRequest
{
    //id of object to sync
    [SerializeField]
    public int ID;
    //serialized transform
    [SerializeField]
    public string transform;

    //construct a sync request
    public SyncRequest(int _id, Transform _transform)
    {
        ID = _id;
        transform = JsonUtility.ToJson(new SerializableTransform(_transform));
    }
}
//request to spawn an object
[Serializable]
public struct SpawnRequest
{
    //index of object to spawn in spawnable objects
    [SerializeField]
    public int Index;
    //does the sender own this object?
    [SerializeField]
    public bool SenderOwns;

    //construct a new spawn request
    public SpawnRequest(int _index, bool _senderOwns)
    {
        Index = _index;
        SenderOwns = _senderOwns;
    }
}
//request to change scenes
[Serializable]
public struct ChangeSceneRequest
{
    //index of the scene to change to
    [SerializeField]
    public int Index;

    //construct a new scene change request
    public ChangeSceneRequest(int _index)
    {
        Index = _index;
    }
}