using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTFFlag : MonoBehaviour
{
    //the team this flag belongs to
    public CTFGameLogic.CTFTeams team;

    //are we being held?
    public bool isBeingHeld;
    //what's holding us?
    private CTFNonVR heldBy;

    //default position and rotation of the flag when we're not being held
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    private void Start()
    {
        //store the default position and rotation of the flag
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    public void FlagPickup(CTFNonVR player)
    {
        //if we're not already being held, become held
        if (!isBeingHeld)
        {
            isBeingHeld = true;
            heldBy = player;
            player.isHoldingFlag = true;
        }
    }

    public void FlagCapture()
    {
        //we're nolonger being held, seeing as we've been captured
        isBeingHeld = false;
        heldBy.isHoldingFlag = false;
        heldBy = null;

        //reset the flag's position and rotation
        transform.position = defaultPosition;
        transform.rotation = defaultRotation;
    }

    public void FlagDrop()
    {
        //we're nolonger being held, seeing as we've been dropped
        isBeingHeld = false;
        heldBy.isHoldingFlag = false;
        heldBy = null;

        //place ourselves directly down from where we are
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo);
        transform.position = hitInfo.point;
    }

    public void Update()
    {
        //if we're being held, follow the player
        if (isBeingHeld)
        {
            transform.position = heldBy.transform.position - heldBy.transform.forward * 0.5f;
            transform.rotation = heldBy.transform.rotation * defaultRotation;
        }
    }
}
