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
        gameClient = FindObjectOfType<MultiClient>();
		
        yield return new WaitForSeconds(1f);

        if(gameClient.inputMethod == InputMethod.VR && !UnityEngine.XR.XRSettings.enabled)
        {
            //enable vr
            UnityEngine.XR.XRSettings.enabled = true;
            UnityEngine.XR.XRSettings.LoadDeviceByName("OpenVR");
            Valve.VR.SteamVR.Initialize(true);
        }
        gameClient.LocalSpawnObject(prefabIDs[(int)gameClient.inputMethod]);
    }

    private void Start()
    {
        StartCoroutine(SpawnPlayer());
    }
}
