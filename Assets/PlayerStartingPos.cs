using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartingPos : MonoBehaviour
{
    public Vector3 VrPos;
    public Vector3 VrRot;
    public Vector3 VrScale = Vector3.one;
    public Vector3 NonVRPos = new Vector3(0, 1.5f, 0);
    public Vector3 NonVRRot;
    public Vector3 NonVRScale = Vector3.one;
}
