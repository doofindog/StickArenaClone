using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PixelManController : NetworkBehaviour, ITickableEntity, IDamageableEntity
{
    [SerializeField] private GameObject _playerSpriteObj;
    [SerializeField] private GameObject _arm;
    
    private PixelManData _data;
    private PlayerInputHandler _playerInputHandler;
    private WeaponComponent _weaponComponent;
    private PixelManAnimator _animator;
    
    public override void OnNetworkSpawn()
    {
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        tickManager.AddEntity(this);

        if (IsClient && IsOwner)
        {
            GameEvents.SendPlayerConnected(this.gameObject);
        }
    }

    public override void OnNetworkDespawn()
    {
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        tickManager.RemoveEntity(this);
    }

    public void Awake()
    {
        _data = GetComponent<PixelManData>();
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _weaponComponent = GetComponent<WeaponComponent>();
        _animator = GetComponent<PixelManAnimator>();
    }

    private void Start()
    {
        _data.Init();
        _weaponComponent.Init();
        if (IsOwner)
        {
            _playerInputHandler.Init(this);
        }
    }

    public void UpdateTick(int tick)
    {
        HandleServerTick();
        HandleClientTick();
    }

    private void HandleServerTick()
    {
        Debug.Log("network id : " + NetworkObjectId +" : " + IsOwner);
        if (IsServer)
        {
            int bufferIndex = -1;

            NetStatePayLoad[] statePayLoads = _data.GetStatePayLoads();
            Queue<NetInputPayLoad> inputsQueue = _data.GetInputQueued();
            
            while (inputsQueue.Count > 0)
            {
                var inputPayLoad = inputsQueue.Dequeue();
                bufferIndex = inputPayLoad.tick % PixelManData.NETWORK_BUFFER_SIZE;

                if (!IsOwner)
                {
                    ProcessMovement(inputPayLoad);
                }

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
    }

    private void HandleClientTick()
    {
        if (IsServer && !IsHost) return;
        
        PerformServerReallocation();
        
        if (IsOwner)
        {
            TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
            NetInputPayLoad[] inputPayLoads = _data.GetInputPayload();
            NetInputPayLoad inputPayLoad = _data.GetCurrentInputPayLoad();

            int bufferIndex = tickManager.GetTick() % PixelManData.NETWORK_BUFFER_SIZE;
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
        
        int serverStateBufferIndex = latestServerState.tick % PixelManData.NETWORK_BUFFER_SIZE;
        //float positionError = Vector3.Distance(latestServerState.position, stateBuffer[serverStateBufferIndex].position);
        //float rotationError = Mathf.Abs(latestServerState.aimAngle - stateBuffer[serverStateBufferIndex].aimAngle);
        
        //NetInputPayLoad[] inputBuffer = _data.GetInputPayload();
        transform.position = latestServerState.position;
        _arm.transform.rotation = Quaternion.Euler(0,0,latestServerState.aimAngle);
        
        SpriteRenderer playerSprite = _playerSpriteObj.GetComponent<SpriteRenderer>();
        bool isFlip = latestServerState.aimAngle is > 90 and < 270;
        
        playerSprite.flipX = isFlip;
        _weaponComponent.FlipWeapon(isFlip);
        
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
        transform.position += inputPayLoad.direction * tickManager.GetMinTickTime() * _data.speed.Value;
        _arm.transform.rotation = Quaternion.Euler(0,0,inputPayLoad.aimAngle);
        
        SpriteRenderer playerSprite = _playerSpriteObj.GetComponent<SpriteRenderer>();
        bool isFlip = inputPayLoad.aimAngle is > 90 and < 270;
        playerSprite.flipX = isFlip;
        _weaponComponent.FlipWeapon(isFlip);
        
        _weaponComponent.UpdateComponent();

        if (IsClient && IsOwner)
        {
            if (inputPayLoad.direction != Vector3.zero)
            {
                _animator.PlayWalk();
            }
            else
            {
                _animator.PlayIdle();
            }
        }
    }
    
    
    public PixelManData GetData()
    {
        return _data;
    }

    public WeaponComponent GetWeaponComponent()
    {
        return _weaponComponent;
    }

    public void TakeDamage(float damage)
    {
        if (IsServer)
        {
            _data.ReduceHealth(damage);
        }

        if (IsOwner)
        {
            _animator.PlayTakeDamage(false);
        }
    }
}
