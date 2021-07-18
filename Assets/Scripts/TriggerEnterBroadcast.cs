using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//https://forum.unity.com/threads/checking-for-a-specific-collider-solved.815118/

[RequireComponent(typeof(Collider))]
public class TriggerEnterBroadcast : MonoBehaviour
{
    // This callback is broadcast to all listeners during OnTriggerEnter.
    public Action<Collider, Collider> onTriggerEntered;

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEntered?.Invoke(GetComponent<Collider>(), other);
    }
}