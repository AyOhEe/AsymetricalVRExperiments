using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

public abstract class GameSystem : MonoBehaviour
{
    //game client
    public MultiClient client;

    //the id of this system
    public int SystemID;

    private void Awake()
    {
        //get the client(if it exists)
        if (FindObjectOfType<MultiClient>())
            client = FindObjectOfType<MultiClient>();
    }

    public abstract void SyncSystem();
    public abstract void HandleMessage(GameSystemData data);

    private void SendMessageToOtherManagers(byte[] message, int type)
    {
        GameSystemData data = new GameSystemData(SystemID, message);
        MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiGameData, MessagePackSerializer.Serialize(data));
        client.SendMessageToServer(MessagePackSerializer.Serialize(baseRequest));
    }

    protected void SendRequest<T>(T request, int requestType)
    {
        //get the team request as bytes
        byte[] serializedRequest = MessagePackSerializer.Serialize(request);
        //store the request with type
        List<byte> serializedRequestWType = new List<byte>(serializedRequest.Length + 1);
        serializedRequestWType.Add((byte)requestType);
        //store the rest of the request
        for (int i = 0; i < serializedRequest.Length; i++)
        {
            serializedRequestWType.Add(serializedRequest[i]);
        }

        //create the systemData request
        GameSystemData systemData = new GameSystemData(SystemID, serializedRequestWType.ToArray());
        //create the base request with the serialized gamesystemdata object
        MultiBaseRequest baseRequest =
            new MultiBaseRequest(MultiPossibleRequest.MultiGameData, MessagePackSerializer.Serialize(systemData));

        //send the serialized request
        client.SendMessageToServer(MessagePackSerializer.Serialize(baseRequest));
    }
}
