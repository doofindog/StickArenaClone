using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArenaNormalState : NetworkBehaviour, IArenaState
{
    private ArenaManager _arenaManager;

    public void Init(ArenaManager arenaManager)
    {
        _arenaManager = arenaManager;
    }
    
    public void OnEnterState()
    {

    }

    public void OnUpdateState()
    {
        
    }

    public void OnExitState()
    {
        
    }
    

    private void SpawnCoinRobots()
    {
        
    }
}
