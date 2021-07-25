using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTFGameManager : GameSystem
{
    public enum CTFTeams
    {
        Red,
        Blue,
        Yellow,
        Green
    };

    public enum CTFData
    {
        SyncScore,
        AssignTeams,
        DamagePlayer,
        KillPlayer,
        FireWeapon,
        ChangeWeapon,

    }

    //flag transforms
    public CTFFlag redFlag;
    public CTFFlag blueFlag;

    //flag pickup zones
    public TriggerEnterBroadcast redPickupZone;
    public TriggerEnterBroadcast bluePickupZone;

    //flag capture zones
    public TriggerEnterBroadcast redCapZone;
    public TriggerEnterBroadcast blueCapZone;

    //team scores
    public int RedScore;
    public int BlueScore;

    // Start is called before the first frame update
    void Start()
    {
        //setup pickup events
        redPickupZone.onTriggerEntered += ((Collider caller, Collider other) =>
        {
            //does the other collider have a CTFPlayer component?
            CTFNonVR player;
            Debug.Log("redPickupZone entered");
            if ((player = other.GetComponent<CTFNonVR>()))
            {
                Debug.Log("redPickupZone entered by CTFPlayer");
                //yes, is it's team blue?
                if (player.player.team == CTFTeams.Blue)
                {
                    Debug.Log("redPickupZone entered by Enemy CTFPlayer");
                    //yes, pickup the flag
                    redFlag.FlagPickup(player);
                }
            }
        });
        bluePickupZone.onTriggerEntered += ((Collider caller, Collider other) =>
        {
            //does the other collider have a CTFPlayer component?
            CTFNonVR player;
            Debug.Log("bluePickupZone entered");
            if ((player = other.GetComponent<CTFNonVR>()))
            {
                Debug.Log("bluePickupZone entered by CTFPlayer");
                //yes, is it's team red?
                if (player.player.team == CTFTeams.Red)
                {
                    Debug.Log("bluePickupZone entered by Enemy CTFPlayer");
                    //yes, pickup the flag
                    blueFlag.FlagPickup(player);
                }
            }
        });

        //setup capture events
        redCapZone.onTriggerEntered += ((Collider caller, Collider other) =>
        {
            //does the other collider have a CTFPlayer component
            CTFNonVR player;
            Debug.Log("redCapZone entered");
            if((player = other.GetComponent<CTFNonVR>()))
            {
                Debug.Log("redCapZone entered by CTFPlayer");
                //yes, is it's team red and does it have a flag?
                if(player.player.team == CTFTeams.Red & player.isHoldingFlag)
                {
                    Debug.Log("POINT RED");
                    RedScore++;
                    //yes, trigger a capture
                    blueFlag.FlagCapture();
                }
            }
        });
        blueCapZone.onTriggerEntered += ((Collider caller, Collider other) =>
        {
            //does the other collider have a CTFPlayer component
            CTFNonVR player;
            Debug.Log("blueCapZone entered");
            if ((player = other.GetComponent<CTFNonVR>()))
            {
                Debug.Log("blueCapZone entered by CTFPlayer");
                //yes, is it's team red and does it have a flag?
                if (player.player.team == CTFTeams.Blue & player.isHoldingFlag)
                {
                    Debug.Log("POINT BLUE");
                    BlueScore++;
                    //yes, trigger a capture
                    redFlag.FlagCapture();
                }
            }
        });


        //get game client
        client = GameObject.FindGameObjectWithTag("GameClient").GetComponent<MultiClient>();
    }

    public override void HandleMessage(GameSystemData message)
    {

    }

    public override void SyncSystem()
    {

    }
}
