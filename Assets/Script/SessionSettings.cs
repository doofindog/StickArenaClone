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

    [Header("Game")] 
    public GameObject defaultWeapon;

    [Header("Score")] 
    public float scoreUpdateTime;
    public int winThreshold;
}
