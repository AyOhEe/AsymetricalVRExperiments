using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float Health { get; private set; } = 100.0f;
    public Vector3 RespawnPoint;
    public Quaternion RespawnRotation;

    public bool canRespawn;

    public void Damage(float damage)
    {
        Health -= damage;
        if (Health < 0)
            Respawn();
    }

    public void Respawn()
    {
        if (canRespawn)
        {
            transform.position = RespawnPoint;
            transform.rotation = RespawnRotation;
        }
    }
}
