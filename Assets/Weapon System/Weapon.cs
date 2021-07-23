using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : ScriptableObject
{
    [Header("Weapon Settings")]
    public GameObject Prefab;
    public string WeaponName;
    public float Damage;

    //creates this weapon as a gameobject
    public GunWeaponComponent SpawnWeapon(Transform parent, PlayerWeaponSystem weaponSystem, bool physics)
    {
        GameObject ret;
        ret = Instantiate(Prefab, parent);
        ret.transform.SetParent(parent);
        ret.GetComponent<GunWeaponComponent>().weaponSystem = weaponSystem;

        if (physics)
        {
            Destroy(ret.GetComponent<Rigidbody>());
            Destroy(ret.GetComponent<MeshCollider>());
        }
        return ret.GetComponent<GunWeaponComponent>();
    }
}
