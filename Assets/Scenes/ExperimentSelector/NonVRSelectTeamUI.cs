using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NonVRSelectTeamUI : MonoBehaviour
{
    private TeamSystem teamSystem;

    [Header("Team Buttons")]
    public Button[] Buttons;
    public TeamSystem.Team[] Teams;

    // Start is called before the first frame update
    void Start()
    {
        teamSystem = FindObjectOfType<TeamSystem>();
        
        //when each button is pressed, change our team to that button's team and destroy this object
        Buttons[0].onClick.AddListener(delegate
        {
            teamSystem.ChangeTeam(Teams[0]);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Destroy(gameObject);
        });
        //when each button is pressed, change our team to that button's team and destroy this object
        Buttons[1].onClick.AddListener(delegate
        {
            teamSystem.ChangeTeam(Teams[1]);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Destroy(gameObject);
        });
    }
}
