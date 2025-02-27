using UnityEngine;

public class Flip : PlayerFeature
{
    private SpriteRenderer m_spriteRenderer;

    public override void Init(NetController pController)
    {
        m_spriteRenderer = pController.playerComponent.spriteRenderer;
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    {
        FlipSprite(pInputPayLoad.aimAngle);
    }

    public override void Process(NetStatePayLoad pStatePayLoad)
    {
        FlipSprite(pStatePayLoad.aimAngle);
    }

    private void FlipSprite(float pAngle)
    {
        bool isFlip = pAngle is > 90 and < 270;
        m_spriteRenderer.flipX = isFlip;
    }
}
