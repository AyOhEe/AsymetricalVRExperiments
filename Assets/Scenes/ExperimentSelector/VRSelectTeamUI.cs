using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSelectTeamUI : MonoBehaviour
{
    private TeamSystem teamSystem;

    [Header("Team Buttons")]
    public PhysicalUIButton[] Buttons;

    // Start is called before the first frame update
    void Start()
    {
        teamSystem = FindObjectOfType<TeamSystem>();

        //iterate through all of the buttons
        for(int i = 0; i < Buttons.Length; i++)
        {
            //when each button is pressed, change our team to that button's team and destroy this object
            Buttons[i].onPress += () =>
            {
                teamSystem.ChangeTeam((TeamSystem.Team)i);
                Destroy(gameObject);
            };
        }
    }
}
