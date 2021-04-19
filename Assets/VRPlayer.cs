using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Update()
    {
        //if the vr object isn't locally owned, disable this object so there is only 1 active camera 
        if (!transform.parent.gameObject.GetComponent<SyncedObject>().localOwned)
            gameObject.SetActive(false);
    }
}
