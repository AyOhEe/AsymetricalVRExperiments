using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerElevator : MonoBehaviour
{
    //the start position of the elevator
    public Vector3 initialPosition;
    //the final position of the elevator
    public Vector3 finalPosition;
    //the position of the elevator last frame
    public Vector3 lastPosition;
    //how long should the elevator take 
    public float overTime;
    //how many iterations should the elevator take to get there?
    public float overIterations;
    //what iteration are we on?
    public float interpolationPoint;

    //list of transforms on the elevator
    public List<Transform> onElevator = new List<Transform>();

    //the scene synced object belonging to this object
    public SceneSyncedObject syncedObject;

    private void Update()
    {
        //only move if we're locally owned(move objects ontop regardless) as the synced object will handle that for us
        if (syncedObject.localOwned)
        {
            //set the position of the transform to where 
            transform.position = Vector3.Lerp(initialPosition, finalPosition, interpolationPoint / overIterations);
        }

        //iterate through all of the transforms on the elevator
        foreach (Transform t in onElevator)
        {
            //move the transform by the same amount the elevator moved this frame
            t.Translate(transform.position - lastPosition);
        }
        //set the last position of the elevator to it's current position, as it will have moved by next frame and will become the last position
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if the object is a nonvrplayer or a hand
        if (other.transform.GetComponent<NonVRPlayerController>() | other.transform.CompareTag("VRRightHand") | other.transform.CompareTag("VRLeftHand"))
        {
            //start moving the elevator if it's not already moving

            //is the elevator at the start of it's path?
            if (interpolationPoint == 0)
                //yeah, so start moving it from the start to the end
                StartCoroutine(InitialToFinal());
            //how about the end of it's path?
            else if (interpolationPoint == overIterations)
                //yeah, so start moving it from the end to the start
                StartCoroutine(FinalToInitial());
        }

        //add the transform to the list of transforms on the elevator, but only if it has a rigidbody
        if (other.transform.GetComponent<Rigidbody>())
            onElevator.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        //if it's a rigidbody, remove it from the list of transforms
        if (other.transform.GetComponent<Rigidbody>())
            onElevator.Remove(other.transform);
    }

    //start 'moving' the elevator from its starting point to it's ending point
    private IEnumerator InitialToFinal()
    {
        //only run if we're locally owned
        if (syncedObject.localOwned)
        {
            //deal with elevator stuff
            for (int i = 0; i < overIterations; i++)
            {
                yield return new WaitForSecondsRealtime(overTime / overIterations);
                Debug.Log("initialtofinal increment");
                interpolationPoint++;
            }
        }
    }

    //start 'moving' the elevator from its ending point to it's starting point
    private IEnumerator FinalToInitial()
    {
        //only run if we're locally owned
        if (syncedObject.localOwned)
        {
            for (int i = 0; i < overIterations; i++)
            {
                yield return new WaitForSecondsRealtime(overTime / overIterations);
                Debug.Log("initialtofinal increment");
                interpolationPoint--;
            }
        }
    }
}
