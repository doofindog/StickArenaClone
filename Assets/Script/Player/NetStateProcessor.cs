using UnityEngine;
using Unity.Netcode;

public class NetStateProcessor : NetworkBehaviour
{
    private const int NETWORK_BUFFER_SIZE = 1024;

    [SerializeField] private NetStatePayLoad[] _statePayLoads = new NetStatePayLoad[NETWORK_BUFFER_SIZE];
    [SerializeField] private NetStatePayLoad _lastProcessedState;

    public void AddState(NetStatePayLoad netStatePayLoad)
    {
        int bufferIndex = TickManager.Instance.GetTick() % NETWORK_BUFFER_SIZE;
        _statePayLoads[bufferIndex] = netStatePayLoad;
    }
    
    public void UpdateLastProcessedState(NetStatePayLoad lastProcessedState)
    {
        _lastProcessedState = lastProcessedState;
    }

    [ClientRpc]
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
}
