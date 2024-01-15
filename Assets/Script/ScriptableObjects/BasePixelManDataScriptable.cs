using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stick Man Arena/Create Player Stats Data")]
public class BasePixelManDataScriptable : ScriptableObject
{
    public float maxHealth;
    public float speed;
    public float dodgeSpeed;
    public float dodgeDuration;
}
