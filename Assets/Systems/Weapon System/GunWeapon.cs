using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AmmoType
{
    Light,
    Medium,
    Heavy,
    Shells
}

[CreateAssetMenu()]
public class GunWeapon : Weapon
{
    [Header("Gun Settings: Attack Method")]
    public bool Hitscan;
    public Vector2 FirePattern;
    public float Spread;
    public float Falloff;
    public float Recoil;
    public GameObject Projectile;

    [Header("Gun Settings: Ammo")]
    public AmmoType AmmoType;
    public int MaxClip;
    public int MaxAmmo;

    [Header("Gun Settings: Reloading")]
    public float ReloadTime;
    public bool IndividualRounds;
    public bool Animated;

    [Header("Gun Settings: Misc")]
    public bool Sighted;
    public float fireSpeed;
}
