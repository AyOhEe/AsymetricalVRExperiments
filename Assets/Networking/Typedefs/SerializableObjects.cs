using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

[MessagePackObject]
public struct SerializableVector3
{
    [Key(0)]
    public float x;
    [Key(1)]
    public float y;
    [Key(2)]
    public float z;

    public SerializableVector3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public SerializableVector3(Vector3 _vector3)
    {
        x = _vector3.x;
        y = _vector3.y;
        z = _vector3.z;
    }

    public Vector3 AsVector3()
    {
        return new Vector3(x, y, z);
    }
}

[MessagePackObject]
public struct SerializableQuaternion
{
    [Key(0)]
    public float x;
    [Key(1)]
    public float y;
    [Key(2)]
    public float z;
    [Key(3)]
    public float w;

    public SerializableQuaternion(float _x, float _y, float _z, float _w)
    {
        x = _x;
        y = _y;
        z = _z;
        w = _w;
    }

    public SerializableQuaternion(Quaternion _quaternion)
    {
        x = _quaternion.x;
        y = _quaternion.y;
        z = _quaternion.z;
        w = _quaternion.w;
    }

    public Quaternion AsQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
}

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
