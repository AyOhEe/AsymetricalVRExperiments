using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicObject : MonoBehaviour
{
    //tag of object to mimic
    public string mTag;

    //the synced object on this object
    public MultiSyncedObject syncedObject;

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

        //make sure we know what we're mimicing
        if (Time.frameCount % 60 == 0)
        {
            //get the object to mimic if it's unknown
            if (!toMimic)
                toMimic = GameObject.FindGameObjectWithTag(mTag);
        }

        //mimic the object's position and rotation
        transform.position = toMimic.transform.position;
        transform.rotation = toMimic.transform.rotation;
    }
}