using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRPlayer : MonoBehaviour
{ 
    private void LevelWasLoaded(Scene current, Scene next)
    {
        //get the scene info object
        GameObject sceneInfo = GameObject.Find("SceneInfo");
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
        SceneManager.activeSceneChanged += LevelWasLoaded;
    }

    void Update()
    {
        //if the vr object isn't locally owned, disable this object so there is only 1 active camera 
        if (!transform.parent.gameObject.GetComponent<SyncedObject>().localOwned)
            gameObject.SetActive(false);
    }
}
