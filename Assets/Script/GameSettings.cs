using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Pixel Arena/Session Setting")]
public class GameSettings : ScriptableObject
{
    [Header("Start up Setting")]
    public int cassetAudioTime;
    public int TurnOnScreenTime;

    [Header("Session Settings")]
    public int maxConnections;
    public int countDownTime;
    public float playerRespawnTime;

    [Header("Game")] 
    public GameObject defaultWeapon;

    [Header("Score")] 
    public float scoreUpdateTime;
    public int winThreshold;

    [Header("Player")]
    public GameObject player;
}
