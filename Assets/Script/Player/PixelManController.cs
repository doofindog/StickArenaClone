using System.Collections;
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
            Debug.Log("called network spawn");
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
        if (!IsServer) return;
        
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
                aimAngle = inputPayLoad.aimAngle,
                dodge = inputPayLoad.dodgePressed
            };
        }

        if (bufferIndex != -1)
        {
            _data.SendStateClientRPC(statePayLoads[bufferIndex]);
        }
    }

    private void HandleClientTick()
    {
        if (IsServer && !IsHost) return;
        
        if (!IsLocalPlayer)
        {
            SimulateMovement();
        }
        else
        {
            PerformServerReallocation();
            
            TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
            NetInputPayLoad[] inputPayLoads = _data.GetInputPayload();
            NetInputPayLoad inputPayLoad = _data.GetNewInputPayLoad();

            int bufferIndex = tickManager.GetTick() % PixelManData.NETWORK_BUFFER_SIZE;
            inputPayLoads[bufferIndex] = inputPayLoad;
            
            ProcessMovement(inputPayLoad);
            
            NetStatePayLoad[] statePayLoads = _data.GetStatePayLoads();
            statePayLoads[bufferIndex] = new NetStatePayLoad()
            {
                tick = inputPayLoad.tick,
                position = transform.position,
                aimAngle = inputPayLoad.aimAngle,
                dodge = inputPayLoad.dodgePressed,
                canDodge = _data.canDodge
            };
            
            _data.SendInputServerRPC(inputPayLoad);
        }
    }

    private void SimulateMovement()
    {
        NetStatePayLoad latestServerState = _data.latestServerStatePayLoad;
        transform.position = latestServerState.position;

        Aim(latestServerState.aimAngle);
    }

    private void PerformServerReallocation()
    {
        NetStatePayLoad serverState = _data.latestServerStatePayLoad;
        NetStatePayLoad clientState = _data.GetStatePayLoadAtTick(serverState.tick);
        
        Vector3 serverPosition = serverState.position;
        Vector3 clientPosition = clientState.position;
        float positionError = Vector3.Distance(serverPosition, clientPosition);
        if (_data.canDodge == false)
        {
            _data.canDodge = serverState.canDodge;
        }
        
        if (positionError > 0.5)
        {
            transform.position = serverState.position;
            NetStatePayLoad[] stateBuffers = _data.GetStatePayLoads();
            stateBuffers[serverState.tick % PixelManData.NETWORK_BUFFER_SIZE] = serverState;

            int tickToProcess = serverState.tick + 1;
            while (tickToProcess < TickManager.Instance.GetTick())
            {
                NetInputPayLoad inputPayLoad = _data.GetInputPayloadAtTick(tickToProcess);
                ProcessMovement(inputPayLoad);
                stateBuffers[tickToProcess % PixelManData.NETWORK_BUFFER_SIZE] = new NetStatePayLoad()
                {
                    tick = inputPayLoad.tick,
                    position = transform.position,
                    aimAngle = inputPayLoad.aimAngle,
                    dodge = inputPayLoad.dodgePressed,
                };
                tickToProcess++;
            }
        }
    }

    private void ProcessMovement(NetInputPayLoad inputPayLoad)
    {
        if (inputPayLoad.dodgePressed && _data.canDodge)
        {
            StartCoroutine(PerformDodge(inputPayLoad.direction));
        }
        
        Move(inputPayLoad.direction);
        Aim(inputPayLoad.aimAngle);
        
        _weaponComponent.UpdateComponent();
        
        UpdateAnimation(inputPayLoad);
    }

    private void Move(Vector3 direction)
    {
        if (_data.state == PixelManData.State.Dodge)
        {
            return;
        }

        _data.state = direction == Vector3.zero ? PixelManData.State.Idle : PixelManData.State.Move;
        
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        transform.position += direction * (tickManager.GetMinTickTime() * _data.speed.Value);
    }
    
    private void Aim(float aimAngle)
    {
        _arm.transform.rotation = Quaternion.Euler(0,0,aimAngle);
        SpriteRenderer playerSprite = _playerSpriteObj.GetComponent<SpriteRenderer>();
        bool isFlip = aimAngle is > 90 and < 270;
        playerSprite.flipX = isFlip;
        _weaponComponent.FlipWeapon(isFlip);
    }

    
    //Send Dodge Data back from the sevrer to stop in the client
    private IEnumerator PerformDodge(Vector3 direction)
    {
        if (_data.state == PixelManData.State.Dodge) yield break;

        _data.canDodge = false;
        
        float timer = 0;
        _data.state = PixelManData.State.Dodge;
        
        while (timer < _data.dodgeDuration.Value)
        {
            TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
            transform.position += direction.normalized * (tickManager.GetMinTickTime() * _data.dodgeSpeed.Value);
             
            yield return new WaitForSeconds(tickManager.GetMinTickTime());

            timer += tickManager.GetMinTickTime();
            
        }
        
        _data.state = PixelManData.State.Idle;

        yield return new WaitForSeconds(2);

        _data.canDodge = true;
    }
    
    private void UpdateAnimation(NetInputPayLoad inputPayLoad)
    {
        if (!IsClient || !IsOwner) return;
        
        if (inputPayLoad.direction != Vector3.zero)
        {
            _animator.PlayWalk();
        }
        else
        {
            _animator.PlayIdle();
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
