using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponComponent : NetworkBehaviour
{
    private const string WEAPON_TAG = "Weapon";
    
    [SerializeField] private Weapon _equippedWeapon;
    
    private StickManController _controller;
    private Weapon _nearByWeapon;

    public void Init()
    {
        TryGetComponent(out _controller);
    }
    
    public void UpdateComponent()
    {
        StickManData data = _controller.GetData();
        if (data.interactPressed)
        {
            TryPickUpWeapon();
        }

        if (data.attackPressed)
        {
            TriggerWeapon();
        }
    }
    
    private void TryPickUpWeapon()
    {
        if(!IsServer && _nearByWeapon == null) return;
        
        EquipWeaponServerRpc();
    }
    
    [ServerRpc]
    private void EquipWeaponServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if(_nearByWeapon == null) return;

        NetworkObject playerNetObj = GetComponent<NetworkObject>();
        ulong senderClientID = playerNetObj.OwnerClientId;
        NetworkObject.ChangeOwnership(senderClientID);
        _equippedWeapon = _nearByWeapon;
        _equippedWeapon.HandleOnEquipped(playerNetObj);
        _nearByWeapon = null;
        EquipWeaponClientRpc(_equippedWeapon.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    private void EquipWeaponClientRpc(NetworkObjectReference weapon)
    {
        if (weapon.TryGet(out NetworkObject targetObject))
        {
            NetworkObject playerNetObj = GetComponent<NetworkObject>();
            _equippedWeapon = targetObject.GetComponent<Weapon>();
            _equippedWeapon.HandleOnEquipped(playerNetObj);
            _nearByWeapon = null;
        }
    }

    private void TriggerWeapon()
    {
        if (IsClient && IsOwner)
        {
            TriggerWeaponServerRpc();
        }
    }

    [ServerRpc]
    private void TriggerWeaponServerRpc()
    {
        _equippedWeapon.Trigger();;
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(WEAPON_TAG))
        {
            if(other.TryGetComponent(out Weapon weapon))
            {
                _nearByWeapon = weapon;
            }
        }
    }
}
