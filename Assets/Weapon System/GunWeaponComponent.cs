using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GunWeaponComponent : WeaponComponent
{
    public GunWeapon weapon;

    private void OnTriggerEnter(Collider other)
    {
        //we should only be picked up if we don't have a weapon system
        if (!weaponSystem)
        {
            //does the other collider have a weaponsystem?
            PlayerWeaponSystem weaponSystem;
            if ((weaponSystem = other.GetComponent<PlayerWeaponSystem>()))
            {
                //yes, add ourselves to the weapon system's list of weapons
                weaponSystem.weapons.Add(weapon);
                //destroy ourselves once we've been picked up, we've passed our data on
                Destroy(gameObject);
            }
        }
    }

    private void Update()
    {
        if (weaponSystem)
        {
            if ((Input.GetMouseButton(0) | Input.GetAxis("MainFire") > 0.5f) & !fireCooldown & weaponSystem.playerController.Player.LocalOwned)
            {
                fireCooldown = true;
                Fire();
                Invoke("RemoveFireCooldown", weapon.fireSpeed);
            }
        }
    }

    protected override void Fire()
    {
        if (weapon.Hitscan)
        {
            Debug.Log("Hitscan Weapon Fire");
            //calculate rotation and deltas
            Quaternion FireDirection = Quaternion.Euler(-0.5f * (weapon.FirePattern.y - 1) * weapon.Spread, -0.5f * (weapon.FirePattern.x - 1) * weapon.Spread, 0);
            Quaternion FireDeltaY = Quaternion.Euler(0, weapon.Spread, 0);
            Quaternion FireResetY = Quaternion.Euler(0, -weapon.FirePattern.x * weapon.Spread, 0);
            Quaternion FireDeltaX = Quaternion.Euler(weapon.Spread, 0, 0);

            //calculate point player camera is looking at
            Ray cameraLookRay = new Ray(weaponSystem.playerController.playerCamera.transform.position,
                                        weaponSystem.playerController.playerCamera.transform.forward);
            Physics.Raycast(cameraLookRay, out RaycastHit camLookHit);
            Vector3 playerLookPoint = camLookHit.point;

            //correct the aim of the firePoint to match the camera
            firePoint.LookAt(camLookHit.point);

            for (int i = 0; i < weapon.FirePattern.x; i++)
            {
                for (int j = 0; j < weapon.FirePattern.y; j++)
                {
                    //fire a ray to scan for hits
                    Ray fireRay = new Ray(firePoint.position, FireDirection * firePoint.rotation * Vector3.forward);
                    Physics.Raycast(fireRay, out RaycastHit hitInfo);
                    Debug.DrawLine(firePoint.position, hitInfo.point);

                    //fire further right next time
                    FireDirection *= FireDeltaY;

                    //if the other object had no collider, just skip tests this time
                    if (!hitInfo.collider)
                        continue;

                    //test for a health system on hit
                    HealthSystem hitHealthSystem;
                    if ((hitHealthSystem = hitInfo.collider.GetComponent<HealthSystem>()))
                    {
                        //there is a health system, deal damage
                        hitHealthSystem.Damage(weapon.Damage - (weapon.Falloff * hitInfo.distance));
                    }

                    //test for a rigidbody on hit
                    Rigidbody hitRB;
                    if((hitRB = hitInfo.collider.GetComponent<Rigidbody>()))
                    {
                        Debug.Log("Hit Rigidbody");
                        //rigidbody found, add force to push object when shot
                        hitRB.AddForce(-hitInfo.normal * weapon.Damage * 10.0f);
                    }
                }

                //reset the direction on the y axis;
                FireDirection *= FireResetY;
                //fire further up next time
                FireDirection *= FireDeltaX;
            }
        }
    }

    void RemoveFireCooldown()
    {
        fireCooldown = false;
    }
}
