using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MessagePack;

public abstract class GamePlayer : MonoBehaviour
{
    public int type;

    //the client this object belongs to
    public int ClientID;
    public MultiClient client;
    
    public bool LocalOwned;

    public void PlayerSetup(int _clientID)
    {
        client = FindObjectOfType<MultiClient>();
        ClientID = _clientID;

        LocalOwned = ClientID == client._ClientID;
        Debug.Log(String.Format("Player {0} Setup", _clientID));
    }
    
    public abstract void SyncPlayer();
    public abstract void HandleMessage(byte[] data);

    protected void SendSyncMessage(byte[] message)
    {
        if (ClientID == client._ClientID)
        {
            MultiSyncPlayer syncPlayer = new MultiSyncPlayer(ClientID, message);
            MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSyncPlayer, MessagePackSerializer.Serialize(syncPlayer));
            client.SendMessageToServer(MessagePackSerializer.Serialize(baseRequest));
        }
    }
}
