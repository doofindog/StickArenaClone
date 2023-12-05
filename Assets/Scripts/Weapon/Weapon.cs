using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private WeaponDataScriptable _weaponData;
    [SerializeField] private GameObject _handObj;
    
    [Header("Weapon Data")]
    [SerializeField] private Transform _barrelTransform;

    [SerializeField] private NetworkVariable<bool> _canAuto;
    [SerializeField] private NetworkVariable<int> _ammoInClip;
    [SerializeField] private NetworkVariable<int> _totalAmmo;

    private ulong playerClientID;
    private bool _canFire;

    public override void OnNetworkSpawn()
    {
        _canFire = true;
        
        if (IsServer)
        {
            _ammoInClip.Value = _weaponData.ammoInClip;
            _totalAmmo.Value = _weaponData.maxAmmo;
        }
    }

    public void HandleOnEquipped(NetworkObject playerNetObj)
    {
        playerClientID = playerNetObj.NetworkObjectId;
            
        GetComponent<NetworkObject>().TrySetParent(playerNetObj.transform);
        GetComponent<BoxCollider2D>().enabled = false;
        
        _barrelTransform.gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = quaternion.identity;
        
        _handObj.SetActive(true);
    }

    public void Trigger()
    {
        if (IsServer && !_canFire || _barrelTransform == null) return;
        
        _canFire = false;
        
        if (_ammoInClip.Value <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        
        GameObject bulletObj = Instantiate(_weaponData.bulletPrefab, _barrelTransform.position, _barrelTransform.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        NetworkObject bulletNetObj = bulletObj.GetComponent<NetworkObject>();
        bullet.Initialise(playerClientID);
        bulletNetObj.Spawn();

        _ammoInClip.Value -= 1;
        
        StartCoroutine(ResetFireRate());
    }

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(_weaponData.reloadTime);

        int ammoToGive = Mathf.Min(_weaponData.ammoInClip - _ammoInClip.Value, _totalAmmo.Value);
        _ammoInClip.Value += ammoToGive;
        _totalAmmo.Value -= ammoToGive;
        if (_totalAmmo.Value < 0)
        {
            _totalAmmo.Value = 0;
        }
        
        _canFire = true;
    }

    private IEnumerator ResetFireRate()
    {
        yield return new WaitForSeconds(_weaponData.fireRate);
        _canFire = true;
    }
}
