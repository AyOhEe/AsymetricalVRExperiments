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
    public Vector3 position;
    //the transform's rotation
    [SerializeField]
    public Quaternion rotation;
    //the transform's scale
    [SerializeField]
    public Vector3 scale;

    //creates a serializable transform based on _transform
    public SerializableTransform(Transform _transform)
    {
        //copy over all attributes
        position = _transform.localPosition;
        rotation = _transform.localRotation;
        scale = _transform.localScale;
    }

    //copies the position, rotation and scale of this transform to the reference transform
    public void CopyToTransform(Transform transform)
    {
        //copy over all attributes
        transform.localPosition = position;
        transform.localRotation = rotation;
        transform.localScale = scale;
    }
}

public class SerializableObjects : MonoBehaviour
{
    private void Start()
    {
    }
}