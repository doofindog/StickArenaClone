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
    Default,
}

[System.Serializable]
public class Team
{
    public string name;
    public TeamType teamType;
    public Color color;
    public int score;
    public List<PlayerData> players;

    public Team(TeamType teamType, Color color)
    {
        name = teamType.ToString();
        this.teamType = teamType;
        this.color = color;
        players = new List<PlayerData>();
    }

    public void AddPlayer(PlayerData player)
    {
        if (player == null)
        {
            Debug.Log("Player Session Data is null ");
            return;
        }
        
        
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
        }
        else
        {
            Destroy(this.gameObject);
        }
        
        CreateTeams();
    }
    
    public void Init()
    {
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

    public void AddPlayerToTeam(PlayerData playerData)
    {
        _teams.Sort((team1, team2)=> team1.players.Count.CompareTo(team2.players.Count));
        _teams[0].AddPlayer(playerData);

        AddPlayerClientRpc(playerData.clientID, playerData.teamType);
    }

    public void AddPlayerToTeam(ulong clientID)
    {
        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        PlayerData playerData = connectionManager.GetPlayerSessionData(clientID);
        AddPlayerToTeam(playerData);
    }

    [ClientRpc]
    private void AddPlayerClientRpc(ulong clientID, TeamType type)
    {
        if(NetworkManager.Singleton.IsHost) return;

        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        PlayerData playerData = connectionManager.GetPlayerSessionData(clientID);
        Team team = GetTeamFromType(type);
        team.AddPlayer(playerData);
    }
    
    public Team GetTeamFromID(ulong clientID)
    {
        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        TeamType teamType = connectionManager.GetPlayerSessionData(clientID).teamType;
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

    public List<Team> GetAllTeams()
    {
        return _teams;
    }

    public void Reset()
    {
        if(_teams == null) return;

        foreach (Team team in _teams)
        {
            team.score = 0;
            team.players.Clear();
        }
    }
}
