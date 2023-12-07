using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Weapon : NetworkBehaviour
{
    [SerializeField] protected WeaponDataScriptable _weaponData;
    
    protected ulong playerClientID;
    protected bool _triggerPressed;
    protected Animator _animator;
    
    public GameObject _equipedWeaponObj;
    public GameObject _unequipedWeaponObj;

    protected virtual void InitialiseData() { }

    public virtual void HandleOnEquipped(NetworkObject playerNetObj)
    {
        playerClientID = playerNetObj.NetworkObjectId;
    }

    public virtual void Trigger() { }
    
    public virtual void ReleaseTrigger()
    {
        if(!_triggerPressed) return;
        
        _triggerPressed = false;
    }

    protected virtual void UpdateAnimation()
    {
        
    }
}
