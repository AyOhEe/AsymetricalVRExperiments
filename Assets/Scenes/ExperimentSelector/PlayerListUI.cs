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
    private Dictionary<string, Tuple<GameObject, TeamSystem.Team>> playerPannels = new Dictionary<string, Tuple<GameObject, TeamSystem.Team>>();
    //we also need to keep track of how many players are in each team
    private Dictionary<TeamSystem.Team, int> playersInTeam = new Dictionary<TeamSystem.Team, int>();

    //prefab for the player pannel
    public GameObject playerPannelPrefab;

    //the button for loading the scene
    public Button LoadLevelButton;

    private void Start()
    {
        //find the team system and store ourselves there
        FindObjectOfType<TeamSystem>().playerListUI = this;

        //get all of the players from the client and create playerpannels for them
        MultiClient client = FindObjectOfType<MultiClient>();
        foreach (int key in client.gamePlayers.Keys)
        {
            AddPlayerToTeamList(client.gamePlayers[key].AsPlayerData(), client.gamePlayers[key].team);
        }

        LoadLevelButton.onClick.AddListener(() =>
        {
            FindObjectOfType<LevelSelectUI>().LoadSelectedScene();
        });
    }

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
        if(!playersInTeam.TryGetValue(newTeam, out _))
        {
            //we aren't, add an entry for the team
            playersInTeam.Add(newTeam, 1);
        }
        //otherwise we can do nothing

        //see if the player pannel exists for this player
        if (playerPannels.TryGetValue(data.PlayerName, out Tuple<GameObject, TeamSystem.Team> pannel))
        {
            //yep, destroy it and recreate it under a new team. and also, remove one member from the old team
            Destroy(pannel.Item1);
            playersInTeam[pannel.Item2]--;
        }

        //create the new pannel
        CreatePannelForPlayer(data, newTeam);
    }

    //creates a player info pannel for the player data given under the team given
    private void CreatePannelForPlayer(PlayerData player, TeamSystem.Team team)
    {
        //spawn the object in
        GameObject instance = Instantiate(playerPannelPrefab, scrollRects[(int)team].GetComponent<ScrollRect>().content);
        //move it down for formatting
        instance.transform.Translate(
            0, 
            -(instance.GetComponent<RectTransform>().rect.height * (playersInTeam[team] - 1)) -((playersInTeam[team] - 1) * 5), 
            0);

        //fill the player pannel in with the data from the playerdata object
        player.FillUIElement(instance.GetComponent<PlayerDescriptionUI>());
        //store it in the playerPannels dictionary
        playerPannels[player.PlayerName] = new Tuple<GameObject, TeamSystem.Team>(instance, team);

        //increase the amount of players in that team
        playersInTeam[team]++;
    }

    private void OnDestroy()
    {
        //find the team system and store ourselves there
        FindObjectOfType<TeamSystem>().playerListUI = null;
    }
}
