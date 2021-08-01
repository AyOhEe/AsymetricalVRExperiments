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

#if UNITY_EDITOR
        Debug.Log(String.Format("Player {0} Spawned in {1}", ClientID, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
#endif
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

#if UNITY_EDITOR
    public void OnDestroy()
    {
        Debug.Log(String.Format("Player {0} Destroyed in {1}", ClientID, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
    }
#endif
}
