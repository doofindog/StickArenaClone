using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetStateProcessor : NetworkBehaviour
{
    private const int NETWORK_BUFFER_SIZE = 1024;

    private NetStatePayLoad[] m_statePayLoads = new NetStatePayLoad[NETWORK_BUFFER_SIZE];
    private NetStatePayLoad m_lastProcessedState;



    public LinkedList<NetStatePayLoad> frameHistory = new LinkedList<NetStatePayLoad>();


    public NetStatePayLoad AddState(NetStatePayLoad pNetStatePayLoad)
    {
        int bufferIndex = pNetStatePayLoad.inputSequence % NETWORK_BUFFER_SIZE;
        m_statePayLoads[bufferIndex] = pNetStatePayLoad;

        return m_statePayLoads[bufferIndex];

    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendProcessedStateClientRPC(NetStatePayLoad[] pNetStatePayLoads)
    {
        if(pNetStatePayLoads.Length == 0)
        {
            return;
        }

        for (int i = 0; i < pNetStatePayLoads.Length; i++)
        {
            int buffeIndex = pNetStatePayLoads[i].inputSequence % NETWORK_BUFFER_SIZE;
            m_statePayLoads[buffeIndex] = pNetStatePayLoads[i];
        }

        m_lastProcessedState = pNetStatePayLoads[pNetStatePayLoads.Length - 1];
    }

    public NetStatePayLoad GetLastProcessedState()
    {
        return m_lastProcessedState;
    }

    public NetStatePayLoad GetStateAtSequenceNumber(int pSequenceNumber)
    {
        int bufferIndex = pSequenceNumber % NETWORK_BUFFER_SIZE;
        return m_statePayLoads[bufferIndex];
    }

    public void UpdateState(NetStatePayLoad statePayLoad)
    {
        int bufferIndex = statePayLoad.tick % NETWORK_BUFFER_SIZE;
        m_statePayLoads[bufferIndex] = statePayLoad;
    }

    public void UpdateStateAtToTick(int tick, NetStatePayLoad statePayLoad)
    {
        int bufferIndex = tick % NETWORK_BUFFER_SIZE;
        m_statePayLoads[bufferIndex] = statePayLoad;
    }
    

    public void OnDrawGizmos()
    {
        foreach (NetStatePayLoad payload in frameHistory)
        {
            Gizmos.DrawWireSphere(payload.position, 0.5f);
        }
    }
}
