using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetInputProcessor : NetworkBehaviour
{
    private const int NETWORK_BUFFER_SIZE = 1024;

    [SerializeField] private NetInputPayLoad[] _inputPayLoads = new NetInputPayLoad[NETWORK_BUFFER_SIZE];
    private Queue<NetInputPayLoad> _inputsQueue = new Queue<NetInputPayLoad>();
    
    public Action<NetInputPayLoad> processedInputEvent; 
    public Action processCompletedEvent; 
    
    [ServerRpc]
    private void SendInputServerRPC(NetInputPayLoad inputPayLoad)
    {
        _inputsQueue.Enqueue(inputPayLoad);
    }
    
    public void AddInput(NetInputPayLoad inputPayLoad)
    {
        int bufferIndex = TickManager.Instance.GetTick() % CharacterDataHandler.NETWORK_BUFFER_SIZE;
        _inputPayLoads[bufferIndex] = inputPayLoad;
        
        SendInputServerRPC(inputPayLoad);
    }

    public void ProcessInputs()
    {
        bool containedInput = _inputsQueue.Count > 0;
        while (_inputsQueue.Count > 0)
        {
            NetInputPayLoad inputPayLoad = _inputsQueue.Dequeue();
            
            processedInputEvent?.Invoke(inputPayLoad);
        }

        if (containedInput)
        {
            processCompletedEvent?.Invoke();
        }
        
    }
    
}
