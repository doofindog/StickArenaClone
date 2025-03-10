using UnityEngine;

public class Shoot : PlayerFeature
{
    public WeaponComponent m_weaponComponent;

    public override void Init(Player pPlayer)
    {
        m_weaponComponent = pPlayer.GetComponent<WeaponComponent>();
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    {
        if(pInputPayLoad.shootPressed == false)
        {
            return;
        }


        Weapon equipedWeapon = m_weaponComponent.GetEquipedWeapon();
        if(equipedWeapon == null)
        {
            Debugger.Log("No Weapon Equipped");
            return;
        };

        Weapon.Params weaponParams = new Weapon.Params()
        {
            tick = 0,
            time = pInputPayLoad.time,
            triggerPressed = pInputPayLoad.shootPressed,
        };

        equipedWeapon.Shoot(weaponParams);
    }

    public override void Process(NetStatePayLoad pStatePayLoad)
    {

    }
}
