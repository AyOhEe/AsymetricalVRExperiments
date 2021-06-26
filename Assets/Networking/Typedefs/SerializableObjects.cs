using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//a serializable representation of a transform
[Serializable]
public struct SerializableTransform
{
    //the transform's position
    [SerializeField]
    public Vector3 p;
    //the transform's rotation
    [SerializeField]
    public Quaternion r;
    //the transform's scale
    [SerializeField]
    public Vector3 s;

    //creates a serializable transform based on _transform
    public SerializableTransform(Transform _transform)
    {
        //copy over all attributes
        p = _transform.localPosition;
        r = _transform.localRotation;
        s = _transform.localScale;
    }

    //copies the position, rotation and scale of this transform to the reference transform
    public void CopyToTransform(Transform transform)
    {
        //copy over all attributes
        transform.localPosition = p;
        transform.localRotation = r;
        transform.localScale = s;
    }
}
