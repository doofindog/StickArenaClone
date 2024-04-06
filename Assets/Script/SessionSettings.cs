using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Pixel Arena/Session Setting")]
public class SessionSettings : ScriptableObject
{
    public int maxConnections;
    public float startGameTime;
    public float prepGameTime;
    public float playerRespawnTime;

    [Header("Dynamic Tiles")]
    public Vector2 pitSize;


    [Header("Score")] 
    public float scoreUpdateTime;
    public int winThreshold;
}
