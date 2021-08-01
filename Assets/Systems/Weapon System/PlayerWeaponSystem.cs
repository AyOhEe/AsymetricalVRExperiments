using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(NonVRPlayerController))]
public class PlayerWeaponSystem : MonoBehaviour
{
    //all weapons held by this weapon system
    public List<Weapon> weapons = new List<Weapon>();

    //weapon parent
    public Transform weaponParent;

    //current weapon
    public WeaponComponent _currentWeapon;
    public int _weaponIndex  = 0;

    //ammo ammounts
    public int LightAmmo;
    public int MediumAmmo;
    public int HeavyAmmo;
    public int ShellsAmmo;

    //non-vr player component
    public NonVRPlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<NonVRPlayerController>();
    }

    private void Update()
    {
        //if we don't have a weapon, select the first weapon
        if(_currentWeapon == null)
        {
            //we need to make sure that weapons actually has something in it
            if(weapons.Count != 0)
            {
                if (weapons[0])
                {
                    _currentWeapon = weapons[0].SpawnWeapon(weaponParent, this, true);
                }
            }
        }

        //keyboard weapons
        if (Input.GetAxis("Mouse ScrollWheel") < 0 & _weaponIndex > 0)
        {
            Destroy(_currentWeapon.gameObject);
            _weaponIndex--;
            _currentWeapon = weapons[_weaponIndex].SpawnWeapon(weaponParent, this, true);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0 & _weaponIndex < weapons.Count - 1)
        {
            Destroy(_currentWeapon.gameObject);
            _weaponIndex++;
            _currentWeapon = weapons[_weaponIndex].SpawnWeapon(weaponParent, this, true);
        }

        //controller weapons
        if (Input.GetButtonDown("LastWeapon") & _weaponIndex > 0)
        {
            Destroy(_currentWeapon.gameObject);
            _weaponIndex--;
            _currentWeapon = weapons[_weaponIndex].SpawnWeapon(weaponParent, this, true);
        }
        if (Input.GetButtonDown("NextWeapon") & _weaponIndex < weapons.Count - 1)
        {
            Destroy(_currentWeapon.gameObject);
            _weaponIndex++;
            _currentWeapon = weapons[_weaponIndex].SpawnWeapon(weaponParent, this, true);
        }
    }
}
