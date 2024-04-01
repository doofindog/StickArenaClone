using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>
{
    [SerializeField] private List<ulong> _crownedPlayerIDs = new List<ulong>();
    [SerializeField] private float _updateTimer;

    public void Awake()
    {
        GameEvents.CrownAcquiredEvent += UpdateCrownedPlayer;
    }
    
    public void Update()
    {
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
                SendTeamWonClientRPC(team.teamType);
            }
        }
    }

    [ClientRpc]
    private void SendTeamWonClientRPC(TeamType teamType)
    {
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
}
