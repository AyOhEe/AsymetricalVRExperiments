using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicObject : MonoBehaviour
{
    //the gamePlayer
    public GamePlayer player;

    public GameObject toMimic;

    public void Update()
    {
        //don't run if this object isn't locally owned
        if (!player.LocalOwned)
            return;
        
        //mimic the object's position and rotation
        transform.position = toMimic.transform.position;
        transform.rotation = toMimic.transform.rotation;
    }
}