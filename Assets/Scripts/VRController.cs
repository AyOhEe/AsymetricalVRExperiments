using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRController : MonoBehaviour
{
    public GameObject cameraRig;
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //get the scene info object
        List<GameObject> sceneObjects = new List<GameObject>();
        scene.GetRootGameObjects(sceneObjects);
        GameObject sceneInfo = sceneObjects[0];
        //get the startingpos info
        PlayerStartingPos startingPos = sceneInfo.GetComponent<PlayerStartingPos>();
        //update our position, rotation and scale
        transform.localPosition = startingPos.VrPos;
        transform.localEulerAngles = startingPos.VrRot;
        transform.localScale = startingPos.VrScale;
    }

    private void Start()
    {
        //add our levelwasloaded event to activeSceneChanged so we can update our transform
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        //get the startingpos info
        PlayerStartingPos startingPos = GameObject.FindObjectOfType<PlayerStartingPos>();

        //update our position, rotation and scale
        transform.localPosition = startingPos.VrPos;
        transform.localEulerAngles = startingPos.VrRot;
        transform.localScale = startingPos.VrScale;

        //if the vr object isn't locally owned, disable this object so there is only 1 active camera 
        if (GetComponent<GamePlayer>().LocalOwned)
        {
            //enable vr
            UnityEngine.XR.XRSettings.enabled = true;
            UnityEngine.XR.XRSettings.LoadDeviceByName("OpenVR");
            Valve.VR.SteamVR.Initialize(true);
            cameraRig.SetActive(true);
        }
    }
}
