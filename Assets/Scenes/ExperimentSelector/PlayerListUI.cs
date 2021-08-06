using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUI : MonoBehaviour
{
    [Header("RectTransforms")]
    //Rect transforms to split the width with
    public List<ScrollRect> scrollRects = new List<ScrollRect>();
    //the Rect transform to fit the lists inside of
    public RectTransform listArea;

    [Header("Spacing Properties")]
    //how much space should be in between each list
    public int padding;

    [Header("Other Variables")]
    //we need to keep track of all of the player panels
    public Dictionary<string, Tuple<GameObject, TeamSystem.Team>> playerPannels = new Dictionary<string, Tuple<GameObject, TeamSystem.Team>>();
    //we also need to keep track of how many players are in each team
    public Dictionary<TeamSystem.Team, int> playersInTeam = new Dictionary<TeamSystem.Team, int>();

    public void Update()
    {
        //iterate through the rectTransforms and set their positions
        for(int i = 0; i < scrollRects.Count; i++)
        {
            scrollRects[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, 
                (listArea.rect.width / scrollRects.Count) - padding);
        }
    }

    public void AddPlayerToTeamList(PlayerData data, TeamSystem.Team newTeam)
    {
        //check if we're keeping track of how many players are in the team
        if(playersInTeam.TryGetValue(newTeam, out _))
        {
            //we aren't, add an entry for the team
            playersInTeam.Add(newTeam, 1);
        }
        //otherwise we can do nothing

        //see if the player pannel exists for this player
        if (playerPannels.TryGetValue(data.PlayerName, out Tuple<GameObject, TeamSystem.Team> pannel))
        {
            //yep, destroy it and recreate it under a new team
            Destroy(pannel.Item1);


        }
    }
}
