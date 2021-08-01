using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

//a serializable representation of a transform
[MessagePackObject]
public struct SerializableTransform
{
    //the transform's position
    [Key(0)]
    public Vector3 p;
    //the transform's rotation
    [Key(1)]
    public Quaternion r;

    //creates a serializable transform based on _transform
    public SerializableTransform(Transform _transform)
    {
        //copy over all attributes
        p = _transform.localPosition;
        r = _transform.localRotation;
    }

    //copies the position, rotation and scale of this transform to the reference transform
    public void CopyToTransform(Transform transform)
    {
        //copy over all attributes
        transform.localPosition = p;
        transform.localRotation = r;
    }
}
