using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnEvent : MonoBehaviour
{
    private LevelSelectUI SelectUI;
    private MultiClient Client;

    private void Start()
    {
        SelectUI = FindObjectOfType<LevelSelectUI>();
        Client = FindObjectOfType<MultiClient>();
    }

    public void Load()
    {
        SelectUI.LoadSelectedScene();
    }
}
