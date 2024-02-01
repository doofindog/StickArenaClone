using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

public class Weapon : NetworkBehaviour
{
    [SerializeField] protected WeaponDataScriptable _weaponData;
    
    protected ulong playerClientID;
    protected bool _triggerPressed;
    protected Animator _animator;
    
    public GameObject _equipedWeaponObj;
    public GameObject _unequipedWeaponObj;

    public NetworkObject weaponOwner;

    protected virtual void InitialiseData() { }

    public virtual void HandleOnEquipped(NetworkObject playerNetObj)
    {
        weaponOwner = playerNetObj;
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

    public virtual bool CanTrigger()
    {
        return false;
    }

    public void Reset()
    {
        weaponOwner = null;
        playerClientID = 0;
        
        ParentConstraint parentConstraint = GetComponent<ParentConstraint>();
        for(int i = 0; i< parentConstraint.sourceCount; i++)
        {
            parentConstraint.RemoveSource(i);
        }
        parentConstraint.constraintActive = false;
        _equipedWeaponObj.SetActive(false);
        _unequipedWeaponObj.SetActive(true);
        transform.rotation = quaternion.Euler(Vector3.zero);
    }
}
