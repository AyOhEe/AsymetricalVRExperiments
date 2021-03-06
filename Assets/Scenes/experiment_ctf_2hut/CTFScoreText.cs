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
    private CTFGameManager gameLogic;
    //the team who's score we're to keep track of
    public TeamSystem.Team team;

    private void Start()
    {
        //get text and game logic
        scoreText = GetComponent<Text>();
        gameLogic = FindObjectOfType<CTFGameManager>();
    }

    private void Update()
    {
        //update the text every 30 frames
        if(Time.frameCount % 30 == 0)
            scoreText.text = team == TeamSystem.Team.A ? gameLogic.RedScore.ToString() : gameLogic.BlueScore.ToString();
    }
}
