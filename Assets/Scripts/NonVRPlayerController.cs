using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BaseNonVRPlayer))]
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
    //the force applied when the player tries to jump 
    public float playerJumpForce;
    //can the player jump?
    public bool jumpEnabled = true;
    //what tags should reset the jump
    public string[] jumpResetTags;

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

    //the player for this player
    public BaseNonVRPlayer Player;

    //Input name for ui interact
    public string UIInteract;
    public bool inMenu;

    //UI to spawn on start if local
    public GameObject UIifLocal;

    //run on the first frame the object is active
    private void Start()
    {
        Player = GetComponent<BaseNonVRPlayer>();

        //make ourselves a cool colour
        int hue = Random.Range(0, 767);
        Color32 color = new Color32(
            (byte)Mathf.Clamp(hue, 0, 255),
            (byte)(Mathf.Clamp(hue, 256, 511) - 256),
            (byte)(Mathf.Clamp(hue, 512, 767) - 256),
            (byte)255);
        GetComponent<MeshRenderer>().material = new Material(GetComponent<MeshRenderer>().material);
        GetComponent<MeshRenderer>().material.SetColor("_Color", color);

        //get the player's rigidbody
        playerRB = playerMainObject.GetComponent<Rigidbody>();

        //don't run if this instance isn't locally owned
        if (!Player.LocalOwned)
        {
            //make the rigidbody kinematic if this isn't the local instance
            playerRB.isKinematic = true;
            //disable the camera too
            playerCamera.SetActive(false);
            return;
        }

        //lock and hide the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //get the startingpos info
        PlayerStartingPos startingPos = GameObject.FindObjectOfType<PlayerStartingPos>();
        if (startingPos)
        {
            //update our position, rotation and scale
            transform.localPosition = startingPos.NonVRPos;
            transform.localEulerAngles = startingPos.NonVRRot;
            transform.localScale = startingPos.NonVRScale;
        }

        //if there's ui to instantiate, instantiate it
        if(UIifLocal)
            Instantiate(UIifLocal);
    }
    
    // Update is called once per frame
    void Update()
    {
        //don't run if this instance isn't locally owned
        if (!Player.LocalOwned)
            return;

        //test if the player opened a menu
        if (Input.GetButtonDown(UIInteract))
        {
            inMenu = !inMenu;
            //lock and hide the mouse
            Cursor.lockState = inMenu ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = inMenu;
        }

        //has the player been moved this frame?
        bool movedThisFrame = false;

        //only  move or rotate if we're not in a menu
        if (!inMenu)
        {
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

            //calculate force directions
            Physics.Raycast(transform.position + transform.forward * 0.001f, Vector3.down, out RaycastHit forwardHit);
            Physics.Raycast(transform.position + transform.right * 0.001f, Vector3.down, out RaycastHit rightHit);
            Physics.Raycast(transform.position - transform.right * 0.001f, Vector3.down, out RaycastHit leftHit);
            Physics.Raycast(transform.position - transform.forward * 0.001f, Vector3.down, out RaycastHit backwardHit);
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit downwardHit);
            Vector3 leftVector = (leftHit.point - downwardHit.point).normalized;
            Vector3 rightVector = (rightHit.point - downwardHit.point).normalized;
            Vector3 forwardVector = (forwardHit.point - downwardHit.point).normalized;
            Vector3 backwardVector = (backwardHit.point - downwardHit.point).normalized;

            if (Input.GetAxis("Horizontal") != 0 | Input.GetAxis("Joystick Horizontal") != 0)
            {
                if ((Input.GetAxis("Horizontal") + -Input.GetAxis("Joystick Horizontal")) > 0)
                {
                    playerRB.AddForce(rightVector * playerAccel * Time.deltaTime * (Input.GetAxis("Horizontal") + Input.GetAxis("Joystick Horizontal")));
                }
                else
                {
                    playerRB.AddForce(-leftVector * playerAccel * Time.deltaTime * (Input.GetAxis("Horizontal") + Input.GetAxis("Joystick Horizontal")));
                }
                movedThisFrame = true;
            }
            if (Input.GetAxis("Vertical") != 0 | Input.GetAxis("Joystick Vertical") != 0)
            {
                if ((Input.GetAxis("Vertical") + -Input.GetAxis("Joystick Vertical")) > 0)
                {
                    playerRB.AddForce(forwardVector * playerAccel * Time.deltaTime * (Input.GetAxis("Vertical") + -Input.GetAxis("Joystick Vertical")));
                }
                else
                {
                    playerRB.AddForce(-backwardVector * playerAccel * Time.deltaTime * (Input.GetAxis("Vertical") + -Input.GetAxis("Joystick Vertical")));
                }
                movedThisFrame = true;
            }
            //Debug.Log("Horizontal: " + (Input.GetAxis("Horizontal") + -Input.GetAxis("Joystick Horizontal")) + ", Vertical: " + (Input.GetAxis("Vertical") + -Input.GetAxis("Joystick Vertical")).ToString());
            //jump if we are allowed to and the player wants to
            if (Input.GetAxis("Jump") != 0 & jumpEnabled)
            {
                //disable jumping until it's reset
                jumpEnabled = false;
                //add a vertical force
                playerRB.AddForce(0, playerJumpForce, 0);
            }
        }
        
        //player movement: clamp velocity magnitude, get the movement axis states then add forces accordingly
        playerRB.velocity = Vector3.ClampMagnitude(new Vector3(playerRB.velocity.x, 0, playerRB.velocity.z), playerSpeedCap)
            + new Vector3(0, playerRB.velocity.y, 0);

        //if there was no keys pressed add a (non-vertical) counterforce to the velocity
        if (!movedThisFrame)
        {
            playerRB.velocity = (new Vector3(playerRB.velocity.x, 0, playerRB.velocity.z) * playerSlowdown) + new Vector3(0, playerRB.velocity.y, 0);
        }
    }

    //reset the jump if we hit something with a tag in jumpResetTags
    public void OnCollisionEnter(Collision collision)
    {
        foreach (string s in jumpResetTags)
        {
            if (collision.transform.CompareTag(s))
                jumpEnabled = true;
        }
    }
}