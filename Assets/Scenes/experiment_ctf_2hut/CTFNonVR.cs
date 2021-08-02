using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CTFPlayer))]
public class CTFNonVR : MonoBehaviour
{
    public CTFPlayer player;
    public Material RedMaterial;
    public Material BlueMaterial;

    //is the player holding a flag?
    public bool isHoldingFlag;

    // Start is called before the first frame update
    void Start()
    {
        //get the ctfplayer
        player = GetComponent<CTFPlayer>();
        //set the material based on the team
        GetComponent<MeshRenderer>().material = player.team == TeamSystem.Team.A ? RedMaterial : BlueMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
