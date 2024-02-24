using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stick Man Arena/Create Weapon Data")]
public class WeaponDataScriptable : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public GameObject bulletPrefab;
    public float damage;
    public float fireRate;
    public float spread;
    public float recoil;
    public int ammoInClip;
    public int maxAmmo;
    public float reloadTime;
    public FireType fireType;
    public float bulletSpeed;
    public int burstBulletCount;
    public float burstFireRate;
}

public enum FireType
{
    Single = 0,
    Burst = 1,
    Auto = 2
}

public enum WeaponState
{
    Ready,
    Fired,
    Reloading,
    ResettingFireRate
}

public enum WeaponType
{
    AssaultRifle,
    SubMachineGun,
    Sniper,
    Handgun,
    Launcher,
}