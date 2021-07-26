using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTFPlayer : GamePlayer
{
    //the team this player is on
    public CTFGameManager.CTFTeams team;

    public override void SyncPlayer()
    {
        throw new System.NotImplementedException();
    }

    public override void HandleMessage(string data)
    {
        throw new System.NotImplementedException();
    }
}
