using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTFGameLogic : MonoBehaviour
{
    //flag transforms
    public Transform redFlag;
    public Transform blueFlag;

    //flag pickup zones
    public Collider redPickupZone;
    public Collider bluePickupZone;

    //flag capture zones
    public Collider redCapZone;
    public Collider blueCapZone;

    //game client
    public MultiClient client;

    // Start is called before the first frame update
    void Start()
    {
        //get game client
        client = GameObject.FindGameObjectWithTag("GameClient").GetComponent<MultiClient>();

        //setup pickup events
    }

    void PotentialPickup(Collider other)
    {
        if(other.GetComponent<>())
    }
}
