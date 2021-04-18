using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Update()
    {
        if (!transform.parent.gameObject.GetComponent<SyncedObject>().localOwned)
            gameObject.SetActive(false);
    }
}
