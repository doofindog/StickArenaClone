using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetStateProcessor : NetworkBehaviour
{
    private const int NETWORK_BUFFER_SIZE = 1024;

    [SerializeField] private NetStatePayLoad[] _statePayLoads = new NetStatePayLoad[NETWORK_BUFFER_SIZE];
    [SerializeField] private NetStatePayLoad _lastProcessedState;

    public LinkedList<NetStatePayLoad> frameHistory = new LinkedList<NetStatePayLoad>();
    private float maxTimeStamp = 0.4f;


    public void AddState(NetStatePayLoad netStatePayLoad)
    {
        int bufferIndex = TickManager.Instance.GetTick() % NETWORK_BUFFER_SIZE;
        _statePayLoads[bufferIndex] = netStatePayLoad;

        if (frameHistory.Count <= 1)
        {
            frameHistory.AddFirst(_statePayLoads[bufferIndex]);
        }
        else
        {
            double historyLength = frameHistory.First.Value.time - frameHistory.Last.Value.time;
            while (historyLength > maxTimeStamp)
            {
                frameHistory.RemoveLast();
                historyLength = frameHistory.First.Value.time - frameHistory.Last.Value.time;
            }

            frameHistory.AddFirst(_statePayLoads[bufferIndex]);
        }
    }
    
    public void UpdateLastProcessedState(NetStatePayLoad lastProcessedState)
    {
        _lastProcessedState = lastProcessedState;
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    public void SendStateClientRpc(NetStatePayLoad state)
    {
        _lastProcessedState = state;
    }

    public NetStatePayLoad GetLastProcessedState()
    {
        return _lastProcessedState;
    }

    public NetStatePayLoad GetStateAtTick(int tick)
    {
        int bufferIndex = tick % NETWORK_BUFFER_SIZE;
        return _statePayLoads[bufferIndex];
    }

    public void UpdateState(NetStatePayLoad statePayLoad)
    {
        int bufferIndex = statePayLoad.tick % NETWORK_BUFFER_SIZE;
        _statePayLoads[bufferIndex] = statePayLoad;
    }

    public void UpdateStateAtToTick(int tick, NetStatePayLoad statePayLoad)
    {
        int bufferIndex = tick % NETWORK_BUFFER_SIZE;
        _statePayLoads[bufferIndex] = statePayLoad;
    }
    

    public void OnDrawGizmos()
    {
        foreach (NetStatePayLoad payload in frameHistory)
        {
            Gizmos.DrawWireSphere(payload.position, 0.5f);
        }
    }
}
