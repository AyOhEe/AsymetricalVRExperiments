using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectSceneOnEvent : MonoBehaviour
{
    public int index;

    private LevelSelectUI SelectUI;
    private MultiClient Client;

    private void Start()
    {
        SelectUI = FindObjectOfType<LevelSelectUI>();
        Client = FindObjectOfType<MultiClient>();
    }

    public void Select()
    {
        SelectUI.SelectScene(Client.possibleScenes[index]);
    }
}
