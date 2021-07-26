using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnEvent : MonoBehaviour
{
    public int sceneIndex;

    private MultiClient client;

    private void Start()
    {
        client = FindObjectOfType<MultiClient>();
    }

    public void LoadScene()
    {
        client.ChangeScene(sceneIndex);
    }
}
