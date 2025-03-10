using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetStateProcessor : NetworkBehaviour
{
    private const int NETWORK_BUFFER_SIZE = 1024;
    private const float MAX_TIME_STAMP = 0.5f;

    private NetStatePayLoad[] m_statePayLoads = new NetStatePayLoad[NETWORK_BUFFER_SIZE];
    private NetStatePayLoad m_lastServerState;

    public LinkedList<NetStatePayLoad> frameHistory = new LinkedList<NetStatePayLoad>();

    public NetStatePayLoad LastProcessedState { get; set; }
    public List<NetStatePayLoad> stateQueue = new List<NetStatePayLoad>();


    public NetStatePayLoad AddState(NetStatePayLoad pNetStatePayLoad)
    {
        int bufferIndex = pNetStatePayLoad.inputSequence % NETWORK_BUFFER_SIZE;
        m_statePayLoads[bufferIndex] = pNetStatePayLoad;

        LastProcessedState = pNetStatePayLoad;

        if (frameHistory.Count <= 1)
        {
            frameHistory.AddFirst(LastProcessedState);
        }
        else
        {
            double historyLength = frameHistory.First.Value.time - frameHistory.Last.Value.time;
            while (historyLength > MAX_TIME_STAMP)
            {
                frameHistory.RemoveLast();
                historyLength = frameHistory.First.Value.time - frameHistory.Last.Value.time;
            }

            frameHistory.AddFirst(LastProcessedState);
        }

        return m_statePayLoads[bufferIndex];
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendProcessedStateClientRPC(NetStatePayLoad[] pNetStatePayLoads)
    {
        if(pNetStatePayLoads.Length == 0)
        {
            return;
        }

        m_lastServerState = pNetStatePayLoads[pNetStatePayLoads.Length - 1];
    }

    public NetStatePayLoad GetServerState()
    {
        return m_lastServerState;
    }

    public NetStatePayLoad GetStateAtSequenceNumber(int pSequenceNumber)
    {
        int bufferIndex = pSequenceNumber % NETWORK_BUFFER_SIZE;
        return m_statePayLoads[bufferIndex];
    }

    public void UpdateState(NetStatePayLoad statePayLoad)
    {
        int bufferIndex = statePayLoad.inputSequence % NETWORK_BUFFER_SIZE;
        m_statePayLoads[bufferIndex] = statePayLoad;
    }

    public void UpdateAtSequence(int sequenceNumber, NetStatePayLoad statePayLoad)
    {
        int bufferIndex =  sequenceNumber % NETWORK_BUFFER_SIZE;
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
