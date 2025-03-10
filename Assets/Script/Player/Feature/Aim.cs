using UnityEngine;

public class Aim : PlayerFeature
{
    private Transform m_arm;
    private WeaponComponent m_weaponComponent;

    public override void Init(Player pPlayer)
    {
        m_arm = pPlayer.arm;
        m_weaponComponent = pPlayer.weaponComponent;
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    {
        ProcessAim(pInputPayLoad.aimAngle);
    }

    public override void Process(NetStatePayLoad pStatePayLoad)
    {
        ProcessAim(pStatePayLoad.aimAngle);
    }

    private void ProcessAim(float aimAngle)
    {
        m_arm.transform.rotation = Quaternion.Euler(0, 0, aimAngle);

        bool isFlip = aimAngle is > 90 and < 270;
        if(m_weaponComponent != null)
        {
            m_weaponComponent.FlipWeapon(isFlip);
        }
    }
}
