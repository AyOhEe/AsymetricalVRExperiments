using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using MessagePack;

public abstract class GamePlayer : MonoBehaviour
{
    public Text nameText;

    public int type;
    public TeamSystem.Team team;

    public string PlayerName;

    //the client this object belongs to
    public int ClientID;
    public MultiClient client;

    private PlayerData data;
    
    public bool LocalOwned;

    public void PlayerSetup(int _clientID, TeamSystem.Team _team, string _name)
    {
        client = FindObjectOfType<MultiClient>();
        ClientID = _clientID;
        team = _team;
        PlayerName = _name;

        nameText.text = PlayerName;

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

    public PlayerData AsPlayerData()
    {
        if (!data)
            data = (PlayerData)PlayerData.CreateInstance(PlayerName);

        data.name = PlayerName;
        data.InputMethod = (InputMethod)type;
        data.Team = team;
        data.Player = this;
        return data;
    }

#if UNITY_EDITOR
    public void OnDestroy()
    {
        Debug.Log(String.Format("Player {0} Destroyed in {1}", ClientID, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
    }
#endif
}
