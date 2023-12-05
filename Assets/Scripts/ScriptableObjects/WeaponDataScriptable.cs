using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stick Man Arena/Create Weapon Data")]
public class WeaponDataScriptable : ScriptableObject
{
    public GameObject bulletPrefab;
    public float fireRate;
    public int ammoInClip;
    public int maxAmmo;
    public float reloadTime;
    public bool canAuto;
}
