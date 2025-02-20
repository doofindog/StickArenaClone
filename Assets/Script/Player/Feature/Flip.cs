using UnityEngine;

public class Flip : PlayerFeature
{
    private CharacterDataHandler m_data;
    private SpriteRenderer m_spriteRenderer;

    public Flip(NetController netController) : base(netController)
    {
        m_data = netController.DataHandler;
        m_spriteRenderer = netController.CharacterSprite;
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    {
        FlipSprite(pInputPayLoad.aimAngle);
    }

    private void FlipSprite(float pAngle)
    {
        bool isFlip = pAngle is > 90 and < 270;
        m_spriteRenderer.flipX = isFlip;
    }
}
