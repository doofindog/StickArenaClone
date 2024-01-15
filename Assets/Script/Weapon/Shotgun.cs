using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Shotgun : RangedWeapon
{
    private const int FIRE_COUNT = 4;
    private const float SPREAD_ANGLE = 15;

    protected override void HandleSingleFire()
    {
        if(_triggerPressed || _weaponState.Value != global::WeaponState.Ready) return;
        
        _triggerPressed = true;
        _weaponState.Value = global::WeaponState.Fired;

        for (int i = 0; i < FIRE_COUNT; i++)
        {
            FireBullet();
        }
        
        StartCoroutine(ResetFireRate());
    }
}
