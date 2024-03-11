using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseGameState : NetworkBehaviour
{
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
}
