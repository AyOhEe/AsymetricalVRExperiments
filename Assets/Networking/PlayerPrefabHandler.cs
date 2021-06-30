using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerPrefabHandler : MonoBehaviour
{
    public int[] prefabIDs = new int[2];
    public MultiClient gameClient;

    private IEnumerator SpawnPlayer()
    {
        yield return new WaitForSeconds(0.2f);

        if (!gameClient)
            gameClient = FindObjectOfType<MultiClient>();
        gameClient.LocalSpawnObject(prefabIDs[(int)gameClient.inputMethod]);
    }

    private void Start()
    {
        StartCoroutine(SpawnPlayer());
    }
}
