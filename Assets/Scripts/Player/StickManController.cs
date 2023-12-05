using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StickManController : NetworkBehaviour, ITickableEntity, IDamageableEntity
{
    [SerializeField] private StickManData _data;
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private WeaponComponent _weaponComponent;
    
    public override void OnNetworkSpawn()
    {
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        tickManager.AddEntity(this);
    }

    public override void OnNetworkDespawn()
    {
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        tickManager.RemoveEntity(this);
    }

    public void Awake()
    {
        _data = GetComponent<StickManData>();
        _inputHandler = GetComponent<InputHandler>();
        _weaponComponent = GetComponent<WeaponComponent>();
    }

    private void Start()
    {
        _data.Init();
        _weaponComponent.Init();
        if (IsOwner)
        {
            _inputHandler.Init(this);
        }
    }

    public void UpdateTick(int tick)
    {
        HandleServerTick();
        HandleClientTick();
    }

    private void HandleServerTick()
    {
        if(!IsServer) return;
        
        int bufferIndex = -1;

        NetStatePayLoad[] statePayLoads = _data.GetStatePayLoads();
        Queue<NetInputPayLoad> inputsQueue = _data.GetInputQueued();
        
        while (inputsQueue.Count > 0)
        {
            var inputPayLoad = inputsQueue.Dequeue();
            bufferIndex = inputPayLoad.tick % StickManData.NETWORK_BUFFER_SIZE;

            ProcessMovement(inputPayLoad);
            statePayLoads[bufferIndex] = new NetStatePayLoad()
            {
                tick = inputPayLoad.tick,
                position = transform.position,
                aimAngle = inputPayLoad.aimAngle
            };
        }

        if (bufferIndex != -1)
        {
            _data.SendStateClientRPC(statePayLoads[bufferIndex]);
        }
    }

    private void HandleClientTick()
    {
        if (!IsClient) return;
        
        PerformServerReallocation();
        
        if (IsOwner)
        {
            TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
            NetInputPayLoad[] inputPayLoads = _data.GetInputPayload();
            NetInputPayLoad inputPayLoad = _data.GetCurrentInputPayLoad();

            int bufferIndex = tickManager.GetTick() % StickManData.NETWORK_BUFFER_SIZE;
            inputPayLoads[bufferIndex] = inputPayLoad;
            
            ProcessMovement(inputPayLoad);
            NetStatePayLoad[] statePayLoads = _data.GetStatePayLoads();
            statePayLoads[bufferIndex] = new NetStatePayLoad()
            {
                tick = inputPayLoad.tick,
                position = transform.position,
                aimAngle = inputPayLoad.aimAngle
            };


            _data.SendInputServerRPC(inputPayLoad);
        }
    }

    private void PerformServerReallocation()
    {
        NetStatePayLoad[] stateBuffer = _data.GetStatePayLoads();
        NetStatePayLoad latestServerState = _data.latestServerStatePayLoad;
        _data.lastProcessedStatePayLoad = latestServerState;
        
        int serverStateBufferIndex = latestServerState.tick % StickManData.NETWORK_BUFFER_SIZE;
        //float positionError = Vector3.Distance(latestServerState.position, stateBuffer[serverStateBufferIndex].position);
        //float rotationError = Mathf.Abs(latestServerState.aimAngle - stateBuffer[serverStateBufferIndex].aimAngle);
        
        //NetInputPayLoad[] inputBuffer = _data.GetInputPayload();
        transform.position = latestServerState.position;
        transform.rotation = Quaternion.Euler(0,0,latestServerState.aimAngle);
        stateBuffer[serverStateBufferIndex] = latestServerState;
        
        //TODO : Server Reallocation
        //Need to look into this setion --------->
        /*int tickToProcess = latestServerState.tick - 1;
        
        while (tickToProcess < TickManager.Instance.GetTick())
        {
            int bufferIndex = tickToProcess % StickManData.NETWORK_BUFFER_SIZE;
            if (bufferIndex != -1)
            {
                ProcessMovement(inputBuffer[bufferIndex]);
                stateBuffer[bufferIndex] = new NetStatePayLoad()
                {
                    tick = inputBuffer[bufferIndex].tick,
                    position = transform.position,
                    aimAngle = latestServerState.aimAngle
                };
            }
                
            tickToProcess++;
        } <-----------*/
    }

    private void ProcessMovement(NetInputPayLoad inputPayLoad)
    {
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        transform.position += inputPayLoad.direction * tickManager.GetMinTickTime() * _data.speed;
        transform.rotation = Quaternion.Euler(0,0,inputPayLoad.aimAngle);

        _weaponComponent.UpdateComponent();
    }
    
    
    public StickManData GetData()
    {
        return _data;
    }

    public WeaponComponent GetWeaponComponent()
    {
        return _weaponComponent;
    }

    public void TakeDamage()
    {
        Debug.Log("Take Damage");
    }
}
