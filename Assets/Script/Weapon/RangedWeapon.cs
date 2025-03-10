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

    public AnticipatedNetworkVariable<int> ammoInClipAnticipated;
    public AnticipatedNetworkVariable<int> totalAmmoAnticipated;
    
    [SerializeField] protected MuzzleFlare muzzleFlare;
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

        GameObject bulletNetObj = ObjectPool.Instance.GetPooledObject(_weaponData.bulletPrefab, barrelTransform.position, barrelTransform.rotation);
        Bullet bullet = bulletNetObj.GetComponent<Bullet>();
        bullet.Initialise(WeaponOwner, _weaponData.damage, _weaponData.bulletSpeed, _weaponParams.time);

        ammoInClip -= 1;
        
        if (WeaponOwner == NetworkManager.Singleton.LocalClientId)
        {
            PlayerEvents.SendWeaponFired();
        }
        
        _animator.Play("Fire");
        weaponState = WeaponState.Fired;
        FireWeaponClientRPC(WeaponOwner);
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

        GameObject bulletNetObj = ObjectPool.Instance.GetPooledObject(_weaponData.bulletPrefab, barrelTransform.position, barrelTransform.rotation);
        Bullet bullet = bulletNetObj.GetComponent<Bullet>();
        bullet.Initialise(WeaponOwner, _weaponData.damage, _weaponData.bulletSpeed);
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
}
