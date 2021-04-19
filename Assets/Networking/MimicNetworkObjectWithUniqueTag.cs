using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicNetworkObjectWithUniqueTag : MonoBehaviour
{
    //tag of object to mimic
    public string mTag;

    //the synced object on this object
    public SyncedObject syncedObject;

    public GameObject toMimic;

    public void Start()
    {
        //don't run if this object isn't locally owned
        if (!syncedObject.localOwned)
            return;

        //object to mimic
        toMimic = GameObject.FindGameObjectWithTag(mTag);
    }

    public void Update()
    {
        //don't run if this object isn't locally owned
        if (!syncedObject.localOwned)
            return;

        //mimic the object's position and rotation
        transform.position = toMimic.transform.position;
        transform.rotation = toMimic.transform.rotation;
    }
}
