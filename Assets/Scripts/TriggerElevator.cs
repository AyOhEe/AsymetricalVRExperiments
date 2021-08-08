using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerElevator : MonoBehaviour
{
    //the start position of the elevator
    public Vector3 initialPosition;
    //the final position of the elevator
    public Vector3 finalPosition;
    //the position of the elevator last frame
    public Vector3 lastPosition;
    //how many fixed update cycles should the elevator take 
    public float overIterations;
    //what iteration are we on?
    public float interpolationPoint;

    //is the elevator going from initialToFinal or from finalToInitial?
    private bool initialToFinal, finalToInitial;

    //list of transforms on the elevator
    public Dictionary<Transform, Transform> onElevator = new Dictionary<Transform, Transform>();

    private void Update()
    {
        //set the position of the transform to where 
        transform.position = Vector3.Lerp(initialPosition, finalPosition, interpolationPoint / overIterations);
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if the object is a nonvrplayer or a hand
        if (other.transform.GetComponent<NonVRPlayerController>() | other.transform.CompareTag("VRHands"))
        {
            //start moving the elevator if it's not already moving

            //is the elevator at the start of it's path?
            if (interpolationPoint == 0)
                //yeah, so start moving it from the start to the end
                initialToFinal = true;
            //how about the end of it's path?
            else if (interpolationPoint == overIterations)
                //yeah, so start moving it from the end to the start
                finalToInitial = true;
        }

        //add the transform to the list of transforms on the elevator, but only if it has a rigidbody
        if (other.transform.GetComponent<Rigidbody>() & !other.transform.CompareTag("VRHands"))
        {
            onElevator.Add(other.transform.parent, other.transform);
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if it's a rigidbody, remove it from the list of transforms
        if (other.transform.GetComponent<Rigidbody>() & !other.transform.CompareTag("VRHands"))
        {
            other.transform.SetParent(onElevator.First(kvp => kvp.Value == other.transform).Key);
            onElevator.Remove(onElevator.First(kvp => kvp.Value == other.transform).Key);
        }
    }

    private void FixedUpdate()
    {
        interpolationPoint += initialToFinal ? 1 : (finalToInitial ? -1 : 0);
        if (interpolationPoint == 0)
        {
            finalToInitial = false;
        }
        else if (interpolationPoint == overIterations)
        {
            initialToFinal = false;
        }
    }
}
