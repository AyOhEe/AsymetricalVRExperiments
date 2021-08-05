using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MessagePack;

public class TeamSystem : GameSystem 
{
    public enum Team
    {
        A,
        B,
        C,
        D
    };
    
    public Team localTeam;
    public Dictionary<int, Team> ClientTeams;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
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
        ChangeTeamRequest
    }
    #endregion Requests

    public override void HandleMessage(GameSystemData data)
    {
        TeamSystemRequest request = (TeamSystemRequest)data.D[0];
        data.D = data.D.AsMemory().Slice(1).ToArray();
        switch (request)
        {
            case TeamSystemRequest.TeamAllocationData:
                TeamAllocationData teamData = MessagePackSerializer.Deserialize<TeamAllocationData>(data.D);
                ClientTeams = teamData.ClientTeams();
                break;
            case TeamSystemRequest.ChangeTeamRequest:
                ChangeTeamRequest teamRequest = MessagePackSerializer.Deserialize<ChangeTeamRequest>(data.D);
                break;
        }
    }

    public override void SyncSystem()
    {
        if (client.hostAuthority)
        {
            TeamAllocationData teamData = new TeamAllocationData(ClientTeams);
            GameSystemData systemData = new GameSystemData(SystemID, MessagePackSerializer.Serialize<TeamAllocationData>(teamData));
            MultiBaseRequest baseRequest = 
                new MultiBaseRequest(MultiPossibleRequest.MultiGameData, MessagePackSerializer.Serialize<GameSystemData>(systemData));
        }
    }

    public void ChangeTeam(Team team)
    {
        ChangeTeamRequest teamRequest = new ChangeTeamRequest(client._ClientID, team);
        GameSystemData systemData = new GameSystemData(SystemID, MessagePackSerializer.Serialize<ChangeTeamRequest>(teamRequest));
        MultiBaseRequest baseRequest = 
            new MultiBaseRequest(MultiPossibleRequest.MultiGameData, MessagePackSerializer.Serialize<GameSystemData>(systemData));
    }
}
