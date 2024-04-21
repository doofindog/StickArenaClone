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
    [SerializeField] protected Transform barrelTransform;
    [SerializeField] protected int ammoInClip;
    [SerializeField] protected int totalAmmo;
    [SerializeField] protected FireType fireType;
    [SerializeField] protected WeaponState weaponState;

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

    public override void Trigger(Weapon.Params inputPayLoad)
    {
        if (totalAmmo == 0 && ammoInClip == 0)
        {
            Debugger.Log("[WEAPON] out of ammo and clips");
            return;
        }
        
        if (ammoInClip <= 0 && weaponState == global::WeaponState.Ready)
        {
            Reload();
            return;
        }

        _weaponParams = inputPayLoad;

        switch (fireType)
        {
            case FireType.Single:
            {
                HandleSingleFire();
                break;
            }
            case FireType.Burst:
            {
                HandleBurstFire();
                break;
            }
            case FireType.Auto:
            {
                HandleAutoFire();
                break;
            }
        }
    }

    protected virtual void HandleSingleFire()
    {
        if(_triggerPressed || weaponState != global::WeaponState.Ready) return;
        
        _triggerPressed = true;
        weaponState = global::WeaponState.Fired;

        FireBullet();
        
        StartCoroutine(ResetFireRate());
    }

    protected virtual void HandleBurstFire()
    {
        if(weaponState != global::WeaponState.Ready) return;

        weaponState = global::WeaponState.Fired;
        _triggerPressed = true;
        
        StartCoroutine(BurstFire());
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < _weaponData.burstBulletCount; i++)
        {
            FireBullet();

            yield return new WaitForSeconds(_weaponData.burstFireRate);
        }

        StartCoroutine(ResetFireRate());
    }

    protected virtual void HandleAutoFire()
    {
        if(weaponState != global::WeaponState.Ready) return;
        
        FireBullet();
        
        StartCoroutine(ResetFireRate());
    }

    protected virtual void FireBullet()
    {
        weaponState = global::WeaponState.Fired;
        
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
        weaponState = global::WeaponState.ResettingFireRate;
        
        yield return new WaitForSeconds(_weaponData.fireRate);
        
        SetWeaponAsReady();
    }
    
    private void SetWeaponAsReady()
    {
        weaponState = global::WeaponState.Ready;
    }

    private void OnWeaponStateChanged(WeaponState oldState, WeaponState newState)
    {
        switch (newState)
        {
            case global::WeaponState.ResettingFireRate:
                if (weaponOwner.IsOwner)
                {
                    
                }
                break;
        }
    }
}
