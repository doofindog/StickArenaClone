using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(PlayerData))]
[RequireComponent(typeof(WeaponComponent))]
[RequireComponent(typeof(PlayerComponent))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerFeatureController))]
[RequireComponent(typeof(NetInputProcessor), typeof(NetStateProcessor))]
public class NetController : NetworkBehaviour
{
    public PlayerData playerData;
    public PlayerInputHandler playerInput;
    public WeaponComponent weaponComponent;
    public PlayerComponent playerComponent;
    public NetInputProcessor inputProcessor;
    public NetStateProcessor stateProcessor;
    public PlayerFeatureController playerFeature;

    public virtual void Awake()
    {   
        playerData = GetComponent<PlayerData>();
        playerInput = GetComponent<PlayerInputHandler>();
        weaponComponent = GetComponent<WeaponComponent>();
        playerComponent = GetComponent<PlayerComponent>();
        inputProcessor = GetComponent<NetInputProcessor>();
        stateProcessor = GetComponent<NetStateProcessor>();
        playerFeature = GetComponent<PlayerFeatureController>();
    }

    public virtual void OnRespawn()
    {

    }

    public virtual void OnDespawn()
    {

    }
}
