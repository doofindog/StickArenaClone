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
    public List<PublicPlayerData> players;

    public Team(TeamType teamType, Color color)
    {
        name = teamType.ToString();
        this.teamType = teamType;
        this.color = color;
        players = new List<PublicPlayerData>();
    }

    public void AddPlayer(PublicPlayerData? pPlayerData)
    {
        if (pPlayerData == null)
        {
            Debug.Log("Player Session Data is null ");
            return;
        }

        PublicPlayerData playerData = pPlayerData.Value;
        playerData.teamType = teamType;
        SessionManager.Instance.UpdatePublicPlayerData(playerData);
        players.Add(playerData);
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
        if (IsClient)
        {
            return;
        }

        AddPlayerToTeam(pClientId);
    }

    public void AddPlayerToTeam(PublicPlayerData playerData)
    {
        m_teamCollection.Sort((team1, team2)=> team1.players.Count.CompareTo(team2.players.Count));
        m_teamCollection[0].AddPlayer(playerData);
    }

    public void AddPlayerToTeam(ulong clientID)
    {
        PublicPlayerData playerData = SessionManager.Instance.GetPublicPlayerData(clientID);
        AddPlayerToTeam(playerData);
    }
    
    public Team GetTeamFromID(ulong clientID)
    {
        SessionManager manager = SessionManager.Instance;
        TeamType teamType = manager.GetPublicPlayerData(clientID).teamType;
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
