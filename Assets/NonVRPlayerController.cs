using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonVRPlayerController : MonoBehaviour
{
    //the acceleration modifier of the player
    public float playerAccel;
    //the slowdown multiplier
    [Range(0.0f, 1.0f)]
    public float playerSlowdown;
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

    //the synced object for this player
    public SyncedObject syncedObject;


    //run on the first frame the object is active
    private void Start()
    {
        //get the player's rigidbody
        playerRB = playerMainObject.GetComponent<Rigidbody>();

        //don't run if this instance isn't locally owned
        if (!syncedObject.localOwned)
        {
            //destroy the rigidbody if this isn't the local instance
            Destroy(playerRB);
            //disable the camera too
            playerCamera.SetActive(false);
            return;
        }

        //lock and hide the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        //don't run if this instance isn't locally owned
        if (!syncedObject.localOwned)
            return;

        //player rotation: get the new desired rotation and rotate the camera and main object
        //get the new desired rotation
        float verticalRot = -Input.GetAxis("Mouse Y") + Input.GetAxis("Joystick Y");
        float horizontalRot = Input.GetAxis("Mouse X") + Input.GetAxis("Joystick X");
        desiredRotationEuler.x = Mathf.Clamp(desiredRotationEuler.x + (verticalRot * cameraRotSpeedUpDown), -maxCamXRot, maxCamXRot);
        desiredRotationEuler.y += horizontalRot * cameraRotSpeedLeftRight;

        //rotate the gameobjects
        playerMainObject.transform.localEulerAngles = new Vector3(0, desiredRotationEuler.y, 0);
        playerCamera.transform.localEulerAngles = new Vector3(desiredRotationEuler.x, 0, 0);
        playerShadesParent.transform.localEulerAngles = new Vector3(desiredRotationEuler.x, 0, 0);

        //has the player been moved this frame?
        bool movedThisFrame = false;
        //player movement: clamp magnitude, get the movement axis states then add forces accordingly
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
            playerRB.velocity = (new Vector3(playerRB.velocity.x, 0, playerRB.velocity.z) * playerSlowdown) + new Vector3(0, playerRB.velocity.y, 0);
        }
    }
}
