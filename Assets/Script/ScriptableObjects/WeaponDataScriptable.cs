using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Stick Man Arena/Create Weapon Data")]
public class WeaponDataScriptable : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public GameObject bulletPrefab;
    public int damage;
    public float fireRate;
    public float spread;
    [Range(-1.0f, 1.0f)]public float[] recoilPattern;
    public int ammoInClip;
    public int maxAmmo;
    public float reloadTime;
    public FireType fireType;
    public float bulletSpeed;
    public int burstBulletCount;
    public float burstFireRate;
    public float chargeTime;
    [FormerlySerializedAs("fireSound")] public AudioClip fireAudio;
}

public enum FireType
{
    Single = 0,
    Burst = 1,
    Auto = 2,
    Charge = 3
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