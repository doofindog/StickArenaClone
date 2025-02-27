using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RangedWeapon : Weapon, IReloadable
{
    [Header("Weapon Data")]
    public int ammoInClip;
    public int totalAmmo;
    
    [SerializeField] protected MuzzleFlare muzzleFlare;
    [SerializeField] protected ChargeEffect _chargeEffect;
    [SerializeField] protected Transform barrelTransform;
    [SerializeField] protected FireType fireType;
    [SerializeField] protected WeaponState weaponState;
    [SerializeField] protected AudioClip fireAudio;
    [SerializeField] protected float chargeTimer;
    [SerializeField] protected bool chargeComplete;

    private Weapon.Params _weaponParams;

    protected NetworkAnimator _netAnimator;

    public override void OnNetworkSpawn()
    {
        InitialiseData();
    }

    public void Update()
    {
        UpdateAnimation();
    }


    protected override void InitialiseData()
    {
        ammoInClip = _weaponData.ammoInClip;
        totalAmmo = _weaponData.maxAmmo;
        fireType = _weaponData.fireType;
        weaponState = global::WeaponState.Ready;
        fireAudio = _weaponData.fireAudio;
        barrelTransform = transform.Find("Barrel");
    }

    public void Start()
    {
        _animator = GetComponent<Animator>();
        _netAnimator = GetComponent<NetworkAnimator>();
        _netAnimator.Animator = _animator;
    }

    public override void HandleOnEquipped(NetworkObject playerNetObj)
    {
        base.HandleOnEquipped(playerNetObj);
        
        barrelTransform.gameObject.SetActive(true);
        _equipedWeaponObj.SetActive(true);
        _unequipedWeaponObj.SetActive(false);
    }

    public override void TriggerPressed(Weapon.Params weaponParams)
    {
        if (totalAmmo == 0 && ammoInClip == 0)
        {
            Debugger.Log("[WEAPON] out of ammo and clips");
            WeaponExhausted();
            return;
        }
        
        if (ammoInClip <= 0 && weaponState == global::WeaponState.Ready)
        {
            Reload();
            return;
        }

        _weaponParams = weaponParams;

        switch (fireType)
        {
            case FireType.Single:
            {
                HandleSingleFire();
                break;
            }
            case FireType.Burst:
            {
                break;
            }
            case FireType.Auto:
            {
                break;
            }
            case FireType.Charge:
            {
                break;
            }
        }
    }

    public override void TriggerReleased()
    {
        base.TriggerReleased();
        
        if (chargeTimer > _weaponData.chargeTime && chargeComplete)
        {
            chargeTimer = 0;
            chargeComplete = false;
        }

        if(chargeTimer > 0 && !chargeComplete )
        {
            chargeTimer -= TickManager.Instance.GetMinTickTime();
        }
    }

    protected virtual void HandleSingleFire()
    {
        if (weaponState != global::WeaponState.Ready)
        {
            return;
        }

        FireBullet();
               
        if(TryGetComponent(out AudioSource source))
        {
            source.PlayOneShot(fireAudio);
        }
        
        StartCoroutine(ResetFireRate());
    }

    protected virtual void FireBullet()
    {
        if (muzzleFlare != null)
        {
            muzzleFlare.Play();
        }
        
        Debugger.Log("[weapon] fire bullet called");

        int index = _weaponParams.tick % _weaponData.recoilPattern.Length;
        float rotation = _weaponData.recoilPattern[index] * _weaponData.spread;
        Quaternion bulletRotation = barrelTransform.rotation * Quaternion.Euler(0, 0, rotation);

        GameObject bulletNetObj = ObjectPool.Instance.GetPooledObject(_weaponData.bulletPrefab, barrelTransform.position, bulletRotation);
        Bullet bullet = bulletNetObj.GetComponent<Bullet>();
        bullet.Initialise(playerClientID, _weaponData.damage, _weaponData.bulletSpeed);

        ammoInClip -= 1;
        
        if (playerClientID == NetworkManager.Singleton.LocalClientId)
        {
            GameEvents.SendWeaponFired();
        }
        
        _animator.Play("Fire");
        weaponState = WeaponState.Fired;
        FireWeaponClientRPC(playerClientID);
        StartCoroutine(ResetFireRate());
    }

    [ClientRpc]
    public void FireWeaponClientRPC(ulong clientID)
    {
        if(clientID == NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        if (muzzleFlare != null)
        {
            muzzleFlare.Play();
        }

        Debugger.Log("[weapon] fire bullet called");

        //int index = _weaponParams.tick % _weaponData.recoilPattern.Length;
        //float rotation = _weaponData.recoilPattern[index] * _weaponData.spread;
        //Quaternion bulletRotation = barrelTransform.rotation * Quaternion.Euler(0, 0, rotation);

        Quaternion bulletRotation = barrelTransform.rotation;

        GameObject bulletNetObj = ObjectPool.Instance.GetPooledObject(_weaponData.bulletPrefab, barrelTransform.position, bulletRotation);
        Bullet bullet = bulletNetObj.GetComponent<Bullet>();
        bullet.Initialise(playerClientID, _weaponData.damage, _weaponData.bulletSpeed);
    }
    

    public void Reload()
    {
        if(weaponState != global::WeaponState.Ready) return;
        
        StartCoroutine(PreformReload());
    }
    
    private IEnumerator PreformReload()
    {
        weaponState = global::WeaponState.Reloading;
        
        yield return new WaitForSeconds(_weaponData.reloadTime);

        int ammoToGive = Mathf.Min(_weaponData.ammoInClip - ammoInClip, totalAmmo);
        ammoInClip += ammoToGive;
        totalAmmo -= ammoToGive;
        totalAmmo = Mathf.Max(totalAmmo, 0);
        
        SetWeaponAsReady();
    }
    
    protected IEnumerator ResetFireRate()
    {        
        yield return new WaitForSeconds(_weaponData.fireRate);
        
        SetWeaponAsReady();
    }
    
    private void SetWeaponAsReady()
    {
        weaponState = global::WeaponState.Ready;
    }

    protected override void UpdateAnimation()
    {
        if(_chargeEffect == null) return;

        if (!chargeComplete)
        {
            _chargeEffect.Charge(chargeTimer, _weaponData.chargeTime);
        }
        else
        {
            _chargeEffect.Charge(0, _weaponData.chargeTime);
        }
    }

    private void OnWeaponStateChanged(WeaponState oldState, WeaponState newState)
    {
        switch (newState)
        {
            case global::WeaponState.Fired:
                if (weaponOwner.IsOwner)
                {
                    
                }
                break;
        }
    }
}
