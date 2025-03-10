using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

public class WeaponComponent : NetworkBehaviour
{
    private const string WEAPON_TAG = "Weapon";

    private Transform m_arm;
    private Transform m_weaponHolder;
    private Weapon m_equippedWeapon;
    private Weapon m_nearByWeapon;


    public void Init(Transform pArm, Transform pWeaponHolder)
    {
        m_arm = pArm;
        m_weaponHolder = pWeaponHolder;
    }

    public void UpdateComponent(NetInputPayLoad inputPayLoad)
    {
        PlayerData dataHandler = GetComponent<PlayerData>();

        if (dataHandler.interactPressed)
        {
            TryPickUpWeapon();
        }
    }
    
    private void TryPickUpWeapon()
    {
        if(!IsServer && m_nearByWeapon == null) return;
        
        SendEquipWeaponServerRpc();
    }
    
    [ServerRpc]
    private void SendEquipWeaponServerRpc(ServerRpcParams serverRpcParams = default)
    {
        EquipWeapon();
    }

    public void EquipWeapon(Weapon weapon = null)
    {
        if (m_equippedWeapon != null)
        {
            DestroyWeapon(m_equippedWeapon);
        }

        m_equippedWeapon = weapon;
        
        ConstraintSource constrainSource = new ConstraintSource
        {
            sourceTransform = m_weaponHolder.transform,
            weight = 1
        };

        ParentConstraint parentConstraint = m_equippedWeapon.GetComponent<ParentConstraint>();
        parentConstraint.AddSource(constrainSource);
        parentConstraint.constraintActive = true;
        
        m_equippedWeapon.GetComponent<BoxCollider2D>().enabled = false;
        
        NetworkObject playerNetObj = GetComponent<NetworkObject>();
        m_equippedWeapon.HandleOnEquipped(playerNetObj);
        
        m_nearByWeapon = null;
        m_equippedWeapon.onWeaponExhausted = GiveDefaultWeapon;

        EquipWeaponClientRpc(m_equippedWeapon.GetComponent<NetworkObject>());
    }

    public void GiveDefaultWeapon()
    {
        GameObject weaponPrefab = GameManager.Instance.GetSessionSettings().defaultWeapon;
        GameObject weaponObj = SpawnManager.Instance.SpawnObject(weaponPrefab, SpawnManager.SpawnType.NETWORK, Vector3.zero,
            Quaternion.identity);
        weaponObj.GetComponent<NetworkObject>().Spawn();
        Weapon weapon = weaponObj.GetComponent<Weapon>();
        
        EquipWeapon(weapon);
    }

    private void DestroyWeapon(Weapon weapon)
    {
        if(!NetworkManager.Singleton.IsServer) return;
        
        weapon.GetComponent<NetworkObject>().Despawn();
    }

    public bool IsWeaponEquipped()
    {
        return m_equippedWeapon != null;
    }


    public void DropEquippedWeapon()
    {
        m_equippedWeapon.GetComponent<NetworkObject>().Despawn();
        m_equippedWeapon = null;
        
        DropEquippedWeaponClientRpc();
    }

    [ClientRpc]
    private void DropEquippedWeaponClientRpc()
    {
        m_equippedWeapon = null;
    }

    [ClientRpc]
    private void EquipWeaponClientRpc(NetworkObjectReference weapon)
    {
        if (!weapon.TryGet(out NetworkObject targetObject))
        {
            Debugger.Log("No networkObject found : " + weapon.NetworkObjectId);
        }

;
        NetworkObject playerNetObj = GetComponent<NetworkObject>();
        m_equippedWeapon = targetObject.GetComponent<Weapon>();

        ConstraintSource constrainSource = new ConstraintSource();
        constrainSource.sourceTransform = m_weaponHolder.transform;
        constrainSource.weight = 1;

        ParentConstraint parentConstraint = m_equippedWeapon.GetComponent<ParentConstraint>();
        parentConstraint.AddSource(constrainSource);
        parentConstraint.constraintActive = true;

        m_equippedWeapon.GetComponent<BoxCollider2D>().enabled = false;

        m_equippedWeapon.HandleOnEquipped(playerNetObj);
        m_nearByWeapon = null;
    }

    public Weapon GetEquipedWeapon()
    {
        return m_equippedWeapon;
    }

    public void FlipWeapon(bool isFlip)
    {
        if (m_equippedWeapon == null) return;

        if(m_equippedWeapon._equipedWeaponObj.TryGetComponent(out SpriteRenderer weaponSprite))
        {
            weaponSprite.flipY = isFlip;
        }
    }
}
