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

    public override void HandleWeapon(Weapon.Params weaponParams)
    {
        base.HandleWeapon(weaponParams);

        if (weaponParams.reloadPressed)
        {
            Reload();
        }
    }

    public override void Trigger(Weapon.Params weaponParams)
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
                HandleBurstFire();
                break;
            }
            case FireType.Auto:
            {
                HandleAutoFire();
                break;
            }
            case FireType.Charge:
            {
                HandleChargeFire();
                break;
            }
        }
    }

    public override void ReleaseTrigger()
    {
        base.ReleaseTrigger();
        
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

    private void HandleChargeFire()
    {
        if(weaponState != global::WeaponState.Ready) return;

        if (!chargeComplete)
        {
            chargeTimer += TickManager.Instance.GetMinTickTime();
            if (!(chargeTimer > _weaponData.chargeTime)) return;
            chargeComplete = true;
            weaponState = global::WeaponState.Fired;
            FireBullet();
            
                    
            if(TryGetComponent(out AudioSource source))
            {
                source.PlayOneShot(fireAudio);
            } 

            StartCoroutine(ResetFireRate());
        }
    }

    protected virtual void HandleSingleFire()
    {
        if(_triggerPressed || weaponState != global::WeaponState.Ready) return;
        
        _triggerPressed = true;
        weaponState = global::WeaponState.Fired;
        FireBullet();
        
                
        if(TryGetComponent(out AudioSource source))
        {
            source.PlayOneShot(fireAudio);
        }
        
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
                    
            if(TryGetComponent(out AudioSource source))
            {
                source.PlayOneShot(fireAudio);
            }
            
            FireBullet();

            yield return new WaitForSeconds(_weaponData.burstFireRate);
        }

        StartCoroutine(ResetFireRate());
    }

    protected virtual void HandleAutoFire()
    {
        if(weaponState != global::WeaponState.Ready) return;
        
        FireBullet();
        
                
        if(TryGetComponent(out AudioSource source))
        {
            source.PlayOneShot(fireAudio);
        }
        
        StartCoroutine(ResetFireRate());
    }

    protected virtual void FireBullet()
    {
        Debug.Log("Called");
        
        if (muzzleFlare != null)
        {
            muzzleFlare.Play();
        }
        
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
            case global::WeaponState.ResettingFireRate:
                if (weaponOwner.IsOwner)
                {
                    
                }
                break;
        }
    }
}
