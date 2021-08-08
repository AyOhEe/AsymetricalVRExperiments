using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NonVRSelectTeamUI : MonoBehaviour
{
    private TeamSystem teamSystem;

    [Header("Team Buttons")]
    public Button[] Buttons;

    // Start is called before the first frame update
    void Start()
    {
        teamSystem = FindObjectOfType<TeamSystem>();

        //iterate through all of the buttons
        for (int i = 0; i < Buttons.Length; i++)
        {
            //when each button is pressed, change our team to that button's team and destroy this object
            Buttons[i].onClick.AddListener(() =>
            {
                teamSystem.ChangeTeam((TeamSystem.Team)i);

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                Destroy(gameObject);
            });
        }
    }
}
