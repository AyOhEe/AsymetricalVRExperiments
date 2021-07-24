using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameManagerBase : MonoBehaviour
{
    //game client
    public MultiClient client;

    private void Awake()
    {
        //get the client(if it exists)
        if (FindObjectOfType<MultiClient>())
            client = FindObjectOfType<MultiClient>();
    }

    //game managers must implement these methods
    public abstract void HandleMessage(GameManagerData message);
    public abstract void SyncGameManager();

    private void SendMessageToOtherManagers(string message, int type)
    {
        GameManagerData data = new GameManagerData(message, type);
        client.SendMessageToServer(JsonUtility.ToJson(data));
    }
}
