using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class ArenaPitfallState : NetworkBehaviour, IArenaState
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
        throw new System.NotImplementedException();
    }

}
