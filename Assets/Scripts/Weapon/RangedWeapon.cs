using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class RangedWeapon : Weapon, IReloadable
{
    [Header("Weapon Data")]
    [SerializeField] private Transform _barrelTransform;
    [SerializeField] private NetworkVariable<int> _ammoInClip;
    [SerializeField] private NetworkVariable<int> _totalAmmo;
    [SerializeField] private NetworkVariable<FireType> _fireType;
    [SerializeField] private NetworkVariable<WeaponState> _weaponState;

    private NetworkAnimator _netAnimator;

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
        if (!IsServer) return;
        
        _ammoInClip.Value = _weaponData.ammoInClip;
        _totalAmmo.Value = _weaponData.maxAmmo;
        _fireType.Value = _weaponData.fireType;
        _weaponState.Value = WeaponState.Ready;
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
        
        _barrelTransform.gameObject.SetActive(true);
        _equipedWeaponObj.SetActive(true);
        _unequipedWeaponObj.SetActive(false);
    }

    public override void Trigger()
    {
        if(!IsServer) return;
        if(_totalAmmo.Value == 0 && _ammoInClip.Value == 0) return;
        
        if (_ammoInClip.Value <= 0 && _weaponState.Value == WeaponState.Ready)
        {
            Reload();
            return;
        }

        switch (_fireType.Value)
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
        if(_triggerPressed || _weaponState.Value != WeaponState.Ready) return;
        
        _triggerPressed = true;
        _weaponState.Value = WeaponState.Fired;

        FireBullet();
        
        StartCoroutine(ResetFireRate());
    }

    protected virtual void HandleBurstFire()
    {
        if(_weaponState.Value != WeaponState.Ready) return;

        _weaponState.Value = WeaponState.Fired;
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
        if(_weaponState.Value != WeaponState.Ready) return;

        _weaponState.Value = WeaponState.Fired;
        
        FireBullet();
        
        StartCoroutine(ResetFireRate());
    }
    
    protected virtual void FireBullet()
    {
        _netAnimator.SetTrigger("fire");
        
        GameObject bulletObj = Instantiate(_weaponData.bulletPrefab, _barrelTransform.position, _barrelTransform.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        NetworkObject bulletNetObj = bulletObj.GetComponent<NetworkObject>();
        bulletNetObj.Spawn();
        bullet.Initialise(playerClientID);

        _ammoInClip.Value -= 1;
    }

    public void Reload()
    {
        if(_weaponState.Value != WeaponState.Ready) return;
        
        StartCoroutine(PreformReload());
    }
    
    private IEnumerator PreformReload()
    {
        _weaponState.Value = WeaponState.Reloading;
        
        yield return new WaitForSeconds(_weaponData.reloadTime);

        int ammoToGive = Mathf.Min(_weaponData.ammoInClip - _ammoInClip.Value, _totalAmmo.Value);
        _ammoInClip.Value += ammoToGive;
        _totalAmmo.Value -= ammoToGive;
        _totalAmmo.Value = Mathf.Max(_totalAmmo.Value, 0);
        
        SetWeaponAsReady();
    }
    
    private IEnumerator ResetFireRate()
    {
        _weaponState.Value = WeaponState.ResettingFireRate;
        
        yield return new WaitForSeconds(_weaponData.fireRate);
        
        SetWeaponAsReady();
    }
    
    private void SetWeaponAsReady()
    {
        _weaponState.Value = WeaponState.Ready;
    }
}
