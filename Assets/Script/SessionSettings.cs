using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Pixel Arena/Session Setting")]
public class SessionSettings : ScriptableObject
{
    public float startGameTime;
    public float prepGameTime;
    public float playerRespawnTime;
    
    [Header("Dynamic Tiles")]
    public int minDynamicTiles;
    public int maxDynamicTiles;
}
