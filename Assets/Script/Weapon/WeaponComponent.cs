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

    [SerializeField] private GameObject _arm;
    [SerializeField] private GameObject _hand;
    
    private Weapon _equippedWeapon;
    private Weapon _nearByWeapon;
    
    public void UpdateComponent(NetInputPayLoad inputPayLoad)
    {
        CharacterDataHandler dataHandler = GetComponent<CharacterDataHandler>();
        
        Aim(inputPayLoad.aimAngle);
        
        if (dataHandler.interactPressed)
        {
            TryPickUpWeapon();
        }

        Weapon.Params weaponParams = new Weapon.Params()
        {
            tick = inputPayLoad.tick,
            triggerPressed = inputPayLoad.attackPressed,
            reloadPressed = dataHandler.reloadPressed
        };
        

        if (_equippedWeapon != null)
        {
            _equippedWeapon.HandleWeapon(weaponParams);
        }
        
    }
    
    private void TryPickUpWeapon()
    {
        if(!IsServer && _nearByWeapon == null) return;
        
        SendEquipWeaponServerRpc();
    }
    
    [ServerRpc]
    private void SendEquipWeaponServerRpc(ServerRpcParams serverRpcParams = default)
    {
        EquipWeapon();
    }

    public void EquipWeapon(Weapon weapon = null)
    {
        if (_equippedWeapon != null)
        {
            DestroyWeapon(_equippedWeapon);
        }

        _equippedWeapon = weapon;
        
        ConstraintSource constrainSource = new ConstraintSource
        {
            sourceTransform = _hand.transform,
            weight = 1
        };

        ParentConstraint parentConstraint = _equippedWeapon.GetComponent<ParentConstraint>();
        parentConstraint.AddSource(constrainSource);
        parentConstraint.constraintActive = true;
        
        _equippedWeapon.GetComponent<BoxCollider2D>().enabled = false;
        
        NetworkObject playerNetObj = GetComponent<NetworkObject>();
        _equippedWeapon.HandleOnEquipped(playerNetObj);
        
        _nearByWeapon = null;
        EquipWeaponClientRpc(_equippedWeapon.GetComponent<NetworkObject>());
    }

    private void DestroyWeapon(Weapon weapon)
    {
        if(!NetworkManager.Singleton.IsServer) return;
        
        weapon.GetComponent<NetworkObject>().Despawn();
    }

    public bool WeaponEquipped()
    {
        return _equippedWeapon != null;
    }


    public void DropEquippedWeapon()
    {
        _equippedWeapon.GetComponent<NetworkObject>().Despawn();
        _equippedWeapon = null;
        
        DropEquippedWeaponClientRpc();
    }

    [ClientRpc]
    private void DropEquippedWeaponClientRpc()
    {
        _equippedWeapon = null;
    }

    [ClientRpc]
    private void EquipWeaponClientRpc(NetworkObjectReference weapon)
    {
        if(IsHost) return;
        
        if (weapon.TryGet(out NetworkObject targetObject))
        {
            NetworkObject playerNetObj = GetComponent<NetworkObject>();
            _equippedWeapon = targetObject.GetComponent<Weapon>();
            
            ConstraintSource constrainSource = new ConstraintSource();
            constrainSource.sourceTransform = _hand.transform;
            constrainSource.weight = 1;
        
            ParentConstraint parentConstraint = _equippedWeapon.GetComponent<ParentConstraint>();
            parentConstraint.AddSource(constrainSource);
            parentConstraint.constraintActive = true;
        
            _equippedWeapon.GetComponent<BoxCollider2D>().enabled = false;
            
            _equippedWeapon.HandleOnEquipped(playerNetObj);
            _nearByWeapon = null;
        }
    }

    public void TriggerWeapon(Weapon.Params weaponParams)
    {
        if (_equippedWeapon == null)
        {
            Debugger.Log("[WEAPON] Weapon Not Equiped");
            return;
        }
            
        _equippedWeapon.Trigger(weaponParams);
    }

    public void ReleaseTrigger()
    {
        if(_equippedWeapon == null) return;
        
        _equippedWeapon.ReleaseTrigger();
    }

    private void ReloadWeapon()
    {
        if (IsClient && IsOwner)
        {
            ReloadPressedServerRpc();
        }
    }

    [ServerRpc]
    private void ReleaseWeaponTriggerServerRpc()
    {
        if(_equippedWeapon == null) return;
        
        _equippedWeapon.ReleaseTrigger();
    }

    [ServerRpc]
    private void ReloadPressedServerRpc()
    {
        if(_equippedWeapon == null) return;

        if (_equippedWeapon is IReloadable reloadableWeapon)
        {
            reloadableWeapon.Reload();
        }
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

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(WEAPON_TAG))
        {
            if(other.TryGetComponent(out Weapon weapon) == _nearByWeapon)
            {
                _nearByWeapon = null;
            }
        }
    }

    public void FlipWeapon(bool isFlip)
    {
        if (_equippedWeapon == null) return;

        if(_equippedWeapon._equipedWeaponObj.TryGetComponent(out SpriteRenderer weaponSprite))
        {
            weaponSprite.flipY = isFlip;
        }
    }
    
    public void Aim(float aimAngle)
    {
        _arm.transform.rotation = Quaternion.Euler(0,0,aimAngle);
        
        bool isFlip = aimAngle is > 90 and < 270;
        FlipWeapon(isFlip);
    }
}
