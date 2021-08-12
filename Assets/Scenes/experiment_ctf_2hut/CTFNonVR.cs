using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTFNonVR : BaseNonVRPlayer
{
    public Material RedMaterial;
    public Material BlueMaterial;

    //is the player holding a flag?
    public bool isHoldingFlag;

    // Start is called before the first frame update
    void Start()
    {
        //set the material based on the team
        GetComponent<MeshRenderer>().material = team == TeamSystem.Team.A ? RedMaterial : BlueMaterial;
    }
}
