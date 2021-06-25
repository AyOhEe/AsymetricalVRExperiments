using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//just a container for all of the synced objects that should be in this scene
public class SceneSyncedObjectList : MonoBehaviour
{
    public GameObject[] sceneSyncedObjects;

    private void Start()
    {
        GameObject.FindGameObjectWithTag("GameClient").GetComponent<MultiClient>().sceneSyncedObjects = new Dictionary<int, MultiSyncedObject>();
        foreach (GameObject g in sceneSyncedObjects)
        {
            Instantiate(g);
        }
    }
}
