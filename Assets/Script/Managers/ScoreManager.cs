using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;
    
    [SerializeField] private List<ulong> _crownedPlayerIDs = new List<ulong>();
    [SerializeField] private float _updateTimer;
    [SerializeField] private TeamType _winningTeam;
    private bool canUpdate;
        
    public void Awake()
    {
        GameEvents.CrownAcquiredEvent += UpdateCrownedPlayer;
        
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public void Update()
    {
        if (!IsServer || canUpdate == false)
        {
            return;
        }

        if (_updateTimer >= GameManager.Instance.GetSessionSettings().scoreUpdateTime)
        {
            UpdateScore();
            _updateTimer = 0;
        }

        _updateTimer += Time.deltaTime;
    }

    private void UpdateScore()
    {
        foreach (var playerID in _crownedPlayerIDs)
        {
            Team team = TeamManager.Instance.GetTeamFromID(playerID);
            if(team == null) continue;
            
            UpdateScoreToTeamClientRPC(team.teamType, 1);
            if (team.score >= GameManager.Instance.GetSessionSettings().winThreshold)
            {
                canUpdate = false;
                _winningTeam = team.teamType;
                SendTeamWonClientRPC(team.teamType);
            }
        }
    }

    [ClientRpc]
    private void SendTeamWonClientRPC(TeamType teamType)
    {
        canUpdate = false;
        _winningTeam = teamType;
        GameEvents.SendTeamWonEvent(teamType);
    }
    
    private void UpdateCrownedPlayer(ulong clientID)
    {
        _crownedPlayerIDs.Add(clientID);
    }
    
    [ClientRpc]
    private void UpdateScoreToTeamClientRPC(TeamType type, int score)
    {
        Team team = TeamManager.Instance.GetTeamFromType(type);
        team.score++;
    }

    private int GetScore(TeamType type)
    {
        return TeamManager.Instance.GetTeamFromType(type).score;
    }

    private TeamType GetPlayerWithHighScore()
    {
        Team highestScoreTeam = null;
        foreach (Team team in TeamManager.Instance.GetAllTeams())
        {
            if (highestScoreTeam == null || team.score > highestScoreTeam.score)
            {
                highestScoreTeam = team;
            }
        }

        return highestScoreTeam.teamType;
    }

    public TeamType GetWinningTeam()
    {
        return _winningTeam;
    }

    public void Reset()
    {
        _crownedPlayerIDs.Clear();
        canUpdate = true;
    }
}
