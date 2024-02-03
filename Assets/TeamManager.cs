using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public enum TeamType
{
    Blue,
    Red,
    Green,
    Yellow,
}

[System.Serializable]
public class Team
{
    public string name;
    public TeamType teamType;
    public Color color;
    public int score;
    public List<PlayerSessionData> players;

    public Team(TeamType teamType, Color color)
    {
        name = teamType.ToString();
        this.teamType = teamType;
        this.color = color;
        players = new List<PlayerSessionData>();
    }

    public void AddPlayer(PlayerSessionData player)
    {
        player.teamType = teamType;
        players.Add(player);
    }
}

public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance { get; private set; }
    
    [SerializeField] private List<Team> _teams = new List<Team>();

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }

        GameEvents.PlayerDiedEvent += UpdateScoreToTeam;
        
        CreateTeams();
    }

    private void CreateTeams()
    {
        _teams ??= new List<Team>();
        _teams.Add(new Team(TeamType.Blue, Color.blue));
        _teams.Add(new Team(TeamType.Yellow, Color.yellow));
        _teams.Add(new Team(TeamType.Red, Color.red));
        _teams.Add(new Team(TeamType.Green, Color.green));
    }

    public void AddPlayerToTeam(PlayerSessionData player)
    {
        _teams.Sort((team1, team2)=> team1.players.Count.CompareTo(team2.players.Count));
        _teams[0].AddPlayer(player);

        AddPlayerClientRpc(player.clientID, player.teamType);
    }

    [ClientRpc]
    private void AddPlayerClientRpc(ulong clientID, TeamType type)
    {
        if(NetworkManager.Singleton.IsHost) return;

        PlayerSessionData playerData = ConnectionManager.Instance.GetPlayerSessionData(clientID);
        Team team = GetTeamFromType(type);
        team.AddPlayer(playerData);
    }
    
    public Team GetTeamFromID(ulong clientID)
    {
        TeamType teamType = ConnectionManager.Instance.GetPlayerSessionData(clientID).teamType;
        return GetTeamFromType(teamType);
    }

    public Team GetTeamFromType(TeamType teamType)
    {
        foreach(Team team in _teams)
        {
            if (team.teamType == teamType)
            {
                return team;
            }
        }

        return null;
    }

    private void UpdateScoreToTeam(NetworkObject killedPlayer, NetworkObject source)
    {
        if (source.gameObject.CompareTag("Player"))
        {
            Team winTeam = GetTeamFromID(source.OwnerClientId);
            UpdateScoreToTeam(winTeam.teamType, 1);
            
            return;
        }
        
        Team killedTeam = GetTeamFromID(killedPlayer.OwnerClientId);
        UpdateScoreToTeam(killedTeam.teamType, -1);
    }
    
    private void UpdateScoreToTeam(TeamType type, int score)
    {
        GetTeamFromType(type).score += score;
        UpdateScoreToTeamClientRPC(type, score);
    }

    [ClientRpc]
    private void UpdateScoreToTeamClientRPC(TeamType type, int score)
    {
        if(IsHost) return;
        
        GetTeamFromType(type).score += 1;
    }
}
