using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponComponent : MonoBehaviour
{
    public PlayerWeaponSystem weaponSystem;

    public Transform firePoint;

    protected bool fireCooldown;

    protected abstract void Fire();
}
