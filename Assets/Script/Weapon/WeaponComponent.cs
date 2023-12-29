using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class WeaponComponent : NetworkBehaviour
{
    private const string WEAPON_TAG = "Weapon";

    [SerializeField] private GameObject _handObj;
    
    private PixelManController _controller;
    private Weapon _equippedWeapon;
    private Weapon _nearByWeapon;

    public void Init()
    {
        TryGetComponent(out _controller);
    }
    
    public void UpdateComponent()
    {
        PixelManData data = _controller.GetData();
        if (data.interactPressed)
        {
            TryPickUpWeapon();
        }

        if (data.attackPressed)
        {
            TriggerWeapon();
        }
        else
        {
            ReleaseTrigger();
        }

        if (data.reloadPressed)
        {
            ReloadWeapon();
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
        
        _equippedWeapon = _nearByWeapon;
        
        ConstraintSource constrainSource = new ConstraintSource
        {
            sourceTransform = _handObj.transform,
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

    [ClientRpc]
    private void EquipWeaponClientRpc(NetworkObjectReference weapon)
    {
        if (weapon.TryGet(out NetworkObject targetObject))
        {
            NetworkObject playerNetObj = GetComponent<NetworkObject>();
            _equippedWeapon = targetObject.GetComponent<Weapon>();
            
            ConstraintSource constrainSource = new ConstraintSource();
            constrainSource.sourceTransform = _handObj.transform;
            constrainSource.weight = 1;
        
            ParentConstraint parentConstraint = _equippedWeapon.GetComponent<ParentConstraint>();
            parentConstraint.AddSource(constrainSource);
            parentConstraint.constraintActive = true;
        
            _equippedWeapon.GetComponent<BoxCollider2D>().enabled = false;
            
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

    private void ReleaseTrigger()
    {
        if (IsClient && IsOwner)
        {
            ReleaseWeaponTriggerServerRpc();
        }
    }

    private void ReloadWeapon()
    {
        if (IsClient && IsOwner)
        {
            ReloadPressedServerRpc();
        }
    }

    [ServerRpc]
    private void TriggerWeaponServerRpc()
    {
        if(_equippedWeapon == null) return;
        
        _equippedWeapon.Trigger();;
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

    public void FlipWeapon(bool isFlip)
    {
        if (_equippedWeapon == null) return;

        if(_equippedWeapon._equipedWeaponObj.TryGetComponent(out SpriteRenderer weaponSprite))
        {
            weaponSprite.flipY = isFlip;
        }
    }
}
