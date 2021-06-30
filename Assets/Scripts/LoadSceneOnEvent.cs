using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnEvent : MonoBehaviour
{
    public string sceneName;

    private MultiClient client;

    private void Start()
    {
        client = FindObjectOfType<MultiClient>();
    }

    public void LoadScene()
    {
        client.ChangeScene(sceneName);
    }
}
