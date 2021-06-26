using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerPrefabHandler : MonoBehaviour
{
    public InputMethod inputMethod;
    public int[] prefabIDs = new int[2];
    public MultiClient gameClient;

    //updates the input method to the value in inputMethodDropdown
    public void UpdateInputMethod(Dropdown dropdown)
    {
        inputMethod = (InputMethod)dropdown.value;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!gameClient)
            gameClient = FindObjectOfType<MultiClient>();
        gameClient.LocalSpawnObject(prefabIDs[(int)inputMethod]);
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
}
