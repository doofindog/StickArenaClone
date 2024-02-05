using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Pixel Arena/Session Setting")]
public class SessionSettings : ScriptableObject
{
    public int gameBeingCountDown;
    public int playerRespawnTime;
    
    [Header("Dynamic Tiles")]
    public int minDynamicTiles;
    public int maxDynamicTiles;
}
