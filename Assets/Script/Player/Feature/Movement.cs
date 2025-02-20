using UnityEngine;

public class Movement : PlayerFeature
{
    private CharacterDataHandler m_data;
    private int m_index;

    public Movement(NetController netController) : base(netController)
    {
        m_data = netController.DataHandler;
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    {
        Move(pInputPayLoad.direction);

        if (pInputPayLoad.direction.magnitude > 0)
        {
            m_index++;
        }
    }

    private void Move(Vector3 pDirection)
    {

        if (m_data.state == CharacterDataHandler.State.Dodge)
        {
            return;
        }

        m_data.state = pDirection == Vector3.zero ? CharacterDataHandler.State.Idle : CharacterDataHandler.State.Move;
        TickManager tickManager = TickManager.Instance;
        transform.position += pDirection * (tickManager.GetMinTickTime() * m_data.speed.Value);
    }
}
