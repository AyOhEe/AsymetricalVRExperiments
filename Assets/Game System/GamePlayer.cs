using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePlayer : MonoBehaviour
{
    public int type;

    //the client this object belongs to
    public int ClientID;
    public MultiClient client;
    
    public bool LocalOwned;

    public void Awake()
    {
        client = FindObjectOfType<MultiClient>();
        client.gamePlayers.Add(ClientID, this);

        LocalOwned = ClientID == client._ClientID;
        Invoke("SyncPlayer", 0.2f);
    }
    
    public abstract void SyncPlayer();
    public abstract void HandleMessage(string data);

    protected void SendSyncMessage(string message)
    {
        if (ClientID == client._ClientID)
        {
            MultiSyncPlayer syncPlayer = new MultiSyncPlayer(ClientID, message);
            MultiBaseRequest baseRequest = new MultiBaseRequest(MultiPossibleRequest.MultiSyncPlayer, JsonUtility.ToJson(syncPlayer));
            client.SendMessageToServer(JsonUtility.ToJson(baseRequest));
        }
    }
}
