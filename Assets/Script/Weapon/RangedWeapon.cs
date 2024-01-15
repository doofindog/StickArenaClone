using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RangedWeapon : Weapon, IReloadable
{
    [Header("Weapon Data")]
    [SerializeField] protected Transform _barrelTransform;
    [SerializeField] protected NetworkVariable<int> _ammoInClip;
    [SerializeField] protected NetworkVariable<int> _totalAmmo;
    [SerializeField] protected NetworkVariable<FireType> _fireType;
    [SerializeField] protected NetworkVariable<WeaponState> _weaponState;

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
        if (IsServer)
        {
            _ammoInClip.Value = _weaponData.ammoInClip;
            _totalAmmo.Value = _weaponData.maxAmmo;
            _fireType.Value = _weaponData.fireType;
            _weaponState.Value = global::WeaponState.Ready;
        }

        if (IsClient)
        {
            _weaponState.OnValueChanged += OnWeaponStateChanged;
        }
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
        
        if (_ammoInClip.Value <= 0 && _weaponState.Value == global::WeaponState.Ready)
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
        if(_triggerPressed || _weaponState.Value != global::WeaponState.Ready) return;
        
        _triggerPressed = true;
        _weaponState.Value = global::WeaponState.Fired;

        FireBullet();
        
        StartCoroutine(ResetFireRate());
    }

    protected virtual void HandleBurstFire()
    {
        if(_weaponState.Value != global::WeaponState.Ready) return;

        _weaponState.Value = global::WeaponState.Fired;
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
        if(_weaponState.Value != global::WeaponState.Ready) return;
        
        FireBullet();
        
        StartCoroutine(ResetFireRate());
    }
    
    protected virtual void FireBullet()
    {
        _weaponState.Value = global::WeaponState.Fired;
        _netAnimator.SetTrigger("fire");

        Quaternion bulletRotation = _barrelTransform.rotation * Quaternion.Euler(0, 0, Random.Range(-_weaponData.spread, _weaponData.spread));
        
        GameObject bulletObj = Instantiate(_weaponData.bulletPrefab, _barrelTransform.position, bulletRotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        NetworkObject bulletNetObj = bulletObj.GetComponent<NetworkObject>();
        bulletNetObj.Spawn();
        bullet.Initialise(playerClientID, _weaponData.damage, _weaponData.bulletSpeed);
        
        _ammoInClip.Value -= 1;
    }

    public void Reload()
    {
        if(_weaponState.Value != global::WeaponState.Ready) return;
        
        StartCoroutine(PreformReload());
    }
    
    private IEnumerator PreformReload()
    {
        _weaponState.Value = global::WeaponState.Reloading;
        
        yield return new WaitForSeconds(_weaponData.reloadTime);

        int ammoToGive = Mathf.Min(_weaponData.ammoInClip - _ammoInClip.Value, _totalAmmo.Value);
        _ammoInClip.Value += ammoToGive;
        _totalAmmo.Value -= ammoToGive;
        _totalAmmo.Value = Mathf.Max(_totalAmmo.Value, 0);
        
        SetWeaponAsReady();
    }
    
    protected IEnumerator ResetFireRate()
    {
        _weaponState.Value = global::WeaponState.ResettingFireRate;
        
        yield return new WaitForSeconds(_weaponData.fireRate);
        
        SetWeaponAsReady();
    }
    
    private void SetWeaponAsReady()
    {
        _weaponState.Value = global::WeaponState.Ready;
    }

    private void OnWeaponStateChanged(WeaponState oldState, WeaponState newState)
    {
        switch (newState)
        {
            case global::WeaponState.ResettingFireRate:
                if (weaponOwner.IsOwner)
                {
                    GameEvents.SendWeaponFired();
                }
                break;
        }
    }
}
