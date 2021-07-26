using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void SendMessageToOtherManagers(string message, int type)
    {
        GameSystemData data = new GameSystemData();
        client.SendMessageToServer(JsonUtility.ToJson(data));
    }
}
