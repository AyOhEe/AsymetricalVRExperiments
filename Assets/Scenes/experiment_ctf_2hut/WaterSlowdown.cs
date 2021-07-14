using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSlowdown : MonoBehaviour
{
    public float dragCoefficient;
    public float angDragCoefficient;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb;
        if(rb = other.GetComponent<Rigidbody>())
        {
            rb.drag *= dragCoefficient;
            rb.angularDrag *= angDragCoefficient;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb;
        if (rb = other.GetComponent<Rigidbody>())
        {
            rb.drag /= dragCoefficient;
            rb.angularDrag /= angDragCoefficient;
        }
    }
}
