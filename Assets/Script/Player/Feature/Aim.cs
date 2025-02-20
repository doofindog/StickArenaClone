using UnityEngine;

public class Aim : PlayerFeature
{
    private Transform m_arm;
    private WeaponComponent m_weaponComponent;

    public Aim(NetController pController) : base(pController)
    {
        m_arm = pController.Arm;
        m_weaponComponent = pController.WeaponComponent;
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    {
        ProcessAim(pInputPayLoad.aimAngle);
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
