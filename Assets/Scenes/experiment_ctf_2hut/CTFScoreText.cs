using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CTFScoreText : MonoBehaviour
{
    //the text object for the score
    private Text scoreText;

    //the ctfgamelogic object
    private CTFGameLogic gameLogic;
    //the team who's score we're to keep track of
    public CTFGameLogic.CTFTeams team;

    private void Start()
    {
        //get text and game logic
        scoreText = GetComponent<Text>();
        gameLogic = FindObjectOfType<CTFGameLogic>();
    }

    private void Update()
    {
        //update the text every 30 frames
        if(Time.frameCount % 30 == 0)
            scoreText.text = team == CTFGameLogic.CTFTeams.Red ? gameLogic.RedScore.ToString() : gameLogic.BlueScore.ToString();
    }
}
