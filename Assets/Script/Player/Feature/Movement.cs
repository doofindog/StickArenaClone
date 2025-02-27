using UnityEngine;

public class Movement : PlayerFeature
{
    private PlayerData m_data;
    private Transform m_transform;
    private Vector3 m_predictedPosition;

    public override void Init(NetController pController)
    {
        m_data = pController.playerData;
        m_transform = pController.transform;
    }

    public override void Process(NetInputPayLoad pInputPayLoad)
    {
        Move(pInputPayLoad.direction);
    }

    public override void Process(NetStatePayLoad pStatePayLoad)
    {
        SimulateMove(pStatePayLoad.position, pStatePayLoad.positionDelta);
    }

    private void Move(Vector3 pDirection)
    {
        m_data.state = pDirection == Vector3.zero ? PlayerData.State.Idle : PlayerData.State.Move;
        TickManager tickManager = TickManager.Instance;
        m_transform.position += pDirection * Time.fixedDeltaTime * m_data.speed.Value;
    }

    private void SimulateMove(Vector3 pPosition, Vector3 pDelta)
    {
        float distance = Vector3.Distance(m_transform.position, pPosition);
        if (distance > 0.05f)
        {
            m_transform.position = Vector3.Lerp(m_transform.position, pPosition, 0.5f);
        }
        else
        {
            m_transform.position += pDelta;
        }
    }
}
