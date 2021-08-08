using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MessagePack;

public class TeamSystem : GameSystem 
{
    //Possible Teams for Players
    public enum Team
    {
        A,
        B,
        C,
        D
    };
    
    //the team of this client
    public Team localTeam;
    //the teams of all clients
    public Dictionary<int, Team> ClientTeams = new Dictionary<int, Team>();

    //select team UI prefabs
    public GameObject SelectTeamUI;
    public GameObject SelectTeamPhysicalUI;

    //the player list ui object in this scene
    public PlayerListUI playerListUI;

    private void Start()
    {
        //the team system has to persist across scenes
        DontDestroyOnLoad(gameObject);

        //when the scene is changed, we should sync the teams if we're host
        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode sceneMode) =>
        {
            SyncSystem();
        };
    }

    #region Requests
    [MessagePackObject]
    public struct TeamAllocationData
    {
        //the client id's
        [Key(0)]
        public int[] c;
        //the team for each of those id's
        [Key(1)]
        public Team[] t;

        public TeamAllocationData(Dictionary<int, Team> clientTeams)
        {
            c = new int[clientTeams.Count];
            t = new Team[clientTeams.Count];
            clientTeams.Keys.CopyTo(c, 0);
            clientTeams.Values.CopyTo(t, 0);
        }

        //get the dictionary containing each client's team
        public Dictionary<int, Team> ClientTeams()
        {
            Dictionary<int, Team> retVal = new Dictionary<int, Team>();
            for (int i = 0; i < c.Length; i++)
            {
                retVal.Add(c[i], t[i]);
            }
            return retVal;
        }
    }

    [MessagePackObject]
    public struct ChangeTeamRequest
    {
        //the client to change Teams
        [Key(0)]
        public int C;

        //the requested team
        [Key(1)]
        public Team R;

        public ChangeTeamRequest(int _C, Team _R)
        {
            C = _C;
            R = _R;
        }
    }
    
    private enum TeamSystemRequest
    {
        TeamAllocationData,
        ChangeTeamRequest,
        SelectTeamRequest,
    }
    #endregion Requests

    public override void HandleMessage(GameSystemData data)
    {
        //get the request type from the first byte in the array
        TeamSystemRequest request = (TeamSystemRequest)data.D[0];
        //then remove that byte from the message to leave it readable
        data.D = data.D.AsMemory().Slice(1).ToArray();

        switch (request)
        {
            //we're receiving the state of the teams
            case TeamSystemRequest.TeamAllocationData:
                TeamAllocationData teamData = MessagePackSerializer.Deserialize<TeamAllocationData>(data.D);
                //store the new team setup
                ClientTeams = teamData.ClientTeams();

                //iterate through all of the clients we have data about
                foreach(int key in ClientTeams.Keys)
                {
                    //change the team
                    client.gamePlayers[key].team = ClientTeams[key];
                    if(playerListUI)
                        playerListUI.AddPlayerToTeamList(client.gamePlayers[key].AsPlayerData(), ClientTeams[key]);
                }
                break;

            //someone wants to change teams
            case TeamSystemRequest.ChangeTeamRequest:
                ChangeTeamRequest teamRequest = MessagePackSerializer.Deserialize<ChangeTeamRequest>(data.D);

                //see if they have a team entry, if not, give them one, if yes, change the entry
                if (ClientTeams.TryGetValue(teamRequest.C, out _))
                    ClientTeams[teamRequest.C] = teamRequest.R;
                else
                    ClientTeams.Add(teamRequest.C, teamRequest.R);
                
                //change the player's team too
                client.gamePlayers[teamRequest.C].team = teamRequest.R;
                //and tell the player list ui to do the same
                if(playerListUI)
                    playerListUI.AddPlayerToTeamList(client.gamePlayers[teamRequest.C].AsPlayerData(), teamRequest.R);
                
                //sync the system if we can
                SyncSystem();

                break;
            
            //we've been asked to select a team, so we need to ask the player
            case TeamSystemRequest.SelectTeamRequest:
                //spawn the correct select team ui depending on the input method
                LevelSelectUI levelUI;
                if (client.inputMethod == InputMethod.VR)
                    Instantiate(SelectTeamPhysicalUI);
                else if (levelUI = FindObjectOfType<LevelSelectUI>())
                    levelUI.OpenNonVRSelectTeamUI(this);
                break;
        }
    }

    //if we're the main team system, we can push our state to the other clients
    public override void SyncSystem()
    {
        if (client.hostAuthority)
        {
            //create the teamallocationdata object
            TeamAllocationData teamData = new TeamAllocationData(ClientTeams);
            SendRequest(teamData, (int)TeamSystemRequest.TeamAllocationData);
        }
    }

    //changes the local team and informs all other team managers of the change
    public void ChangeTeam(Team team)
    {
        //store the new local team
        localTeam = team;

        client.gamePlayers[client._ClientID].team = team;

        //create the changeteamrequest object
        ChangeTeamRequest teamRequest = new ChangeTeamRequest(client._ClientID, team);
        SendRequest(teamRequest, (int)TeamSystemRequest.ChangeTeamRequest);

        //update the playerlist if it exists
        if (playerListUI)
            playerListUI.AddPlayerToTeamList(client.gamePlayers[client._ClientID].AsPlayerData(), team);
    }

    //change team from an integer instead of an enum
    public void ChangeTeam(int team)
    {
        //but we just cast it to an enum anyway
        ChangeTeam((Team)team);
    }

    //sends a request to all team managers to tell them to select a team
    public void SendSelectTeamRequest()
    {
        if (client.hostAuthority)
        {
            //store the system id and request type in the gamesystemdata object
            GameSystemData systemData = new GameSystemData(SystemID, new byte[1] { (byte)TeamSystemRequest.SelectTeamRequest });
            //create the base request with the serialized gamesystemdata object
            MultiBaseRequest baseRequest =
                new MultiBaseRequest(MultiPossibleRequest.MultiGameData, MessagePackSerializer.Serialize<GameSystemData>(systemData));

            //send the serialized request
            client.SendMessageToServer(MessagePackSerializer.Serialize<MultiBaseRequest>(baseRequest));
        }
    }
}
