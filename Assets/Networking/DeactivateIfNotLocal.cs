using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateIfNotLocal : MonoBehaviour
{
    public List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();
    public SyncedObject syncedObject;

    private void Update()
    {
        if (!syncedObject.localOwned)
        {
            foreach (MonoBehaviour mono in monoBehaviours)
            {
                mono.enabled = false;
            }
        }
    }
}
