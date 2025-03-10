using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



[RequireComponent(typeof(Player))]
public class NetController : MonoBehaviour
{
    protected Player player;

    public virtual void Awake()
    {   
        player = GetComponent<Player>();
    }

    public virtual void Init()
    {

    }

    public virtual void OnRespawn()
    {

    }

    public virtual void OnDespawn()
    {

    }
}
