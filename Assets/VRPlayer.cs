using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRPlayer : MonoBehaviour
{
    public GameObject cameraRig;

    IEnumerator SetTransformInfo(Scene scene, LoadSceneMode mode)
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

        yield return new WaitForSeconds(0.1f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetTransformInfo(scene, mode));
    }

    private void Start()
    {
        //add our levelwasloaded event to activeSceneChanged so we can update our transform
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        //get the startingpos info
        PlayerStartingPos startingPos = GameObject.FindObjectOfType<PlayerStartingPos>();
        if (startingPos)
        {
            //update our position, rotation and scale
            transform.localPosition = startingPos.VrPos;
            transform.localEulerAngles = startingPos.VrRot;
            transform.localScale = startingPos.VrScale;
        }
    }

    void Update()
    {
        //if the vr object isn't locally owned, disable this object so there is only 1 active camera 
        if (!GetComponent<SyncedObject>().localOwned)
        {
            cameraRig.SetActive(false);
        }
    }
}
