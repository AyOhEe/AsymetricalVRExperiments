using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerElevator : MonoBehaviour
{
    public Vector3 initialPosition;
    public Vector3 finalPosition;
    public Vector3 lastPosition;
    public float overTime;
    public float overIterations;
    public float interpolationPoint;

    public List<Transform> onElevator = new List<Transform>();

    private void Update()
    {
        
        transform.position = Vector3.Lerp(initialPosition, finalPosition, interpolationPoint / overIterations);
        foreach (Transform t in onElevator)
        {
            t.Translate(transform.position - lastPosition);
        }
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<NonVRPlayerController>())
        {
            if (interpolationPoint == 0)
                StartCoroutine(InitialToFinal());
            else
                StartCoroutine(FinalToInitial());

            onElevator.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        onElevator.Remove(other.transform);
    }

    private IEnumerator InitialToFinal()
    {
        for (int i = 0; i < overIterations; i++)
        {
            yield return new WaitForSeconds(overTime / overIterations);
            interpolationPoint++;
        }
    }

    private IEnumerator FinalToInitial()
    {
        for (int i = 0; i < overIterations; i++)
        {
            yield return new WaitForSeconds(overTime / overIterations);
            interpolationPoint--;
        }
    }
}
