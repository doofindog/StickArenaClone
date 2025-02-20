using System;
using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameUtilt
{
    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Camera.main.farClipPlane * 0.5f;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        
        return mouseWorldPos;
    }

    public static void ExecuteNetworkCode(Action pServer, Action pClient)
    {
#if SERVER
        pServer?.Invoke();

#elif CLIENT
        pClient?.Invoke();

#elif UNITY_EDITOR
        string[] multiplayTag = CurrentPlayer.ReadOnlyTags();
        if (multiplayTag.Contains("CLIENT"))
        {
            pClient?.Invoke();
        }
        else if (multiplayTag.Contains("SERVER"))
        {
            pServer?.Invoke();
        }
#endif
    }
}
