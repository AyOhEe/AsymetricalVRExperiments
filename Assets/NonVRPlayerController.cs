﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonVRPlayerController : MonoBehaviour
{
    //the acceleration modifier of the player
    public float playerAccel;
    //the maximum rotation of the camera on the x axis
    public float maxCamXRot;
    //rotation speed multipliers
    public float cameraRotSpeedLeftRight, cameraRotSpeedUpDown;
    //player speed cap
    public float playerSpeedCap;

    //the parent of the "shades"
    public GameObject playerShadesParent;
    //the camera gameobject of the player
    public GameObject playerCamera;
    //the gameobject of the player containing the rigidbody
    public GameObject playerMainObject;

    //the RigidBody belonging to the player main object
    private Rigidbody playerRB;
    //the desired euler rotation of the rigidbody
    private Vector3 desiredRotationEuler;


    //run on the first frame the object is active
    private void Start()
    {
        //get the player's rigidbody
        playerRB = playerMainObject.GetComponent<Rigidbody>();
    }
    
    // Update is called once per frame
    void Update()
    {
        //player rotation: get the new desired rotation and rotate the camera and main object
        //get the new desired rotation
        float verticalRot = -Input.GetAxis("Mouse Y") + Input.GetAxis("Joystick Y");
        float horizontalRot = Input.GetAxis("Mouse X") + Input.GetAxis("Joystick X");
        desiredRotationEuler.x = Mathf.Clamp(desiredRotationEuler.x + (verticalRot * cameraRotSpeedUpDown), -maxCamXRot, maxCamXRot);
        desiredRotationEuler.y += horizontalRot * cameraRotSpeedLeftRight;
        Debug.Log(new Vector2(horizontalRot, verticalRot).ToString());

        //rotate the gameobjects
        playerMainObject.transform.localEulerAngles = new Vector3(0, desiredRotationEuler.y, 0);
        playerCamera.transform.localEulerAngles = new Vector3(desiredRotationEuler.x, 0, 0);
        playerShadesParent.transform.localEulerAngles = new Vector3(desiredRotationEuler.x, 0, 0);

        //has the player been moved this frame?
        bool movedThisFrame = false;
        //player movement: clamp magnitude, get the movement key states then add forces accordingly
        playerRB.velocity = Vector3.ClampMagnitude(new Vector3(playerRB.velocity.x, 0, playerRB.velocity.z), playerSpeedCap)
            + new Vector3(0, playerRB.velocity.y, 0);
        if (Input.GetAxis("Horizontal") != 0 | Input.GetAxis("Joystick Horizontal") != 0)
        {
            playerRB.AddForce(transform.right * playerAccel * Time.deltaTime * (Input.GetAxis("Horizontal") + Input.GetAxis("Joystick Horizontal")));
            movedThisFrame = true;
        }
        if (Input.GetAxis("Vertical") != 0 | Input.GetAxis("Joystick Vertical") != 0)
        {
            playerRB.AddForce(transform.forward * playerAccel * Time.deltaTime * (Input.GetAxis("Vertical") + -Input.GetAxis("Joystick Vertical")));
            movedThisFrame = true;
        }

        //if there was no keys pressed add a (horizontal) counterforce to the velocity
        if (!movedThisFrame)
        {
            playerRB.AddForce(new Vector3(-playerRB.velocity.x * 1.0f, 0.0f, -playerRB.velocity.z * 1.0f), ForceMode.Acceleration);
        }
    }
}