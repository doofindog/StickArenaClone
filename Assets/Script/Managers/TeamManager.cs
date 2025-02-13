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
    public List<ClientData> players;

    public Team(TeamType teamType, Color color)
    {
        name = teamType.ToString();
        this.teamType = teamType;
        this.color = color;
        players = new List<ClientData>();
    }

    public void AddPlayer(ClientData? pClientData)
    {
        if (pClientData == null)
        {
            Debug.Log("Player Session Data is null ");
            return;
        }

        ClientData clientData = pClientData.Value;
        clientData.publicPlayerData.teamType = teamType;
        players.Add(clientData);
    }
}

public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance { get; private set; }
    
    [SerializeField] private List<Team> m_teamCollection = new List<Team>();

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
    }

    public void Init()
    {
        m_teamCollection ??= new List<Team>();
        m_teamCollection.Add(new Team(TeamType.Blue, Color.blue));
        m_teamCollection.Add(new Team(TeamType.Yellow, Color.yellow));
        m_teamCollection.Add(new Team(TeamType.Red, Color.red));
        m_teamCollection.Add(new Team(TeamType.Green, Color.green));

        NetworkManager.OnClientConnectedCallback += HandleOnClientConnected;
    }

    private void HandleOnClientConnected(ulong pClientId)
    {
        AddPlayerToTeam(pClientId);
    }

    public void AddPlayerToTeam(ClientData playerData)
    {
        m_teamCollection.Sort((team1, team2)=> team1.players.Count.CompareTo(team2.players.Count));
        m_teamCollection[0].AddPlayer(playerData);
    }

    public void AddPlayerToTeam(ulong clientID)
    {
        ClientData playerData = SessionManager.Instance.GetClientData(clientID);
        AddPlayerToTeam(playerData);
    }
    
    public Team GetTeamFromID(ulong clientID)
    {
        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        TeamType teamType = TeamType.Default;
        return GetTeamFromType(teamType);
    }

    public Team GetTeamFromType(TeamType teamType)
    {
        foreach(Team team in m_teamCollection)
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
        return m_teamCollection;
    }

    public Team GetTeamData(TeamType pTeamType)
    {
        for (int i = 0; i < m_teamCollection.Count; i++) 
        {
            if(m_teamCollection[i].teamType == pTeamType)
            {
                return m_teamCollection[i];
            }
        }

        return null;
    }

    public void Clean(bool stopped)
    {
        if(m_teamCollection == null) return;

        foreach (Team team in m_teamCollection)
        {
            team.score = 0;
            team.players.Clear();
        }
    }
}
