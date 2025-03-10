using System;
using UnityEngine;

public class Death : PlayerFeature
{
    private PlayerAnimationController m_animController;

    public override void Init(Player pController)
    {
        m_animController = pController.playerAnimController;

        pController.playerData.health.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if(newValue == 0)
        {
            m_animController.PlayDeathAnimation(false);
        }
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    { 

    }

    public override void Process(NetStatePayLoad pStatePayLoad)
    {

    }
}
