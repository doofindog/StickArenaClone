using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour, ITickableEntity, IDamageableEntity
{
    [SerializeField] private GameObject _playerSpriteObj;
    [SerializeField] private GameObject _arm;
    
    private CharacterDataHandler _dataHandler;
    private PlayerInputHandler _playerInputHandler;
    private WeaponComponent _weaponComponent;
    private CharacterAnimator _animator;
    private NetInputProcessor _netInputProcessor;
    
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
        _dataHandler = GetComponent<CharacterDataHandler>();
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _weaponComponent = GetComponent<WeaponComponent>();
        _animator = GetComponent<CharacterAnimator>();
    }

    private void Start()
    {
        _dataHandler.Init();
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

        NetStatePayLoad[] statePayLoads = _dataHandler.GetStatePayLoads();
        Queue<NetInputPayLoad> inputsQueue = _dataHandler.GetInputQueued();
            
        while (inputsQueue.Count > 0)
        {
            var inputPayLoad = inputsQueue.Dequeue();
            bufferIndex = inputPayLoad.tick % CharacterDataHandler.NETWORK_BUFFER_SIZE;

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
            _dataHandler.SendStateClientRPC(statePayLoads[bufferIndex]);
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
            NetInputPayLoad[] inputPayLoads = _dataHandler.GetInputPayload();
            NetInputPayLoad inputPayLoad = _dataHandler.GetNewInputPayLoad();

            int bufferIndex = tickManager.GetTick() % CharacterDataHandler.NETWORK_BUFFER_SIZE;
            inputPayLoads[bufferIndex] = inputPayLoad;
            
            ProcessMovement(inputPayLoad);
            
            NetStatePayLoad[] statePayLoads = _dataHandler.GetStatePayLoads();
            statePayLoads[bufferIndex] = new NetStatePayLoad()
            {
                tick = inputPayLoad.tick,
                position = transform.position,
                aimAngle = inputPayLoad.aimAngle,
                dodge = inputPayLoad.dodgePressed,
                canDodge = _dataHandler.canDodge
            };
            
            _dataHandler.SendInputServerRPC(inputPayLoad);
        }
    }

    private void SimulateMovement()
    {
        NetStatePayLoad latestServerState = _dataHandler.GetLastSeverStatePayLoad();
        transform.position = latestServerState.position;

        Aim(latestServerState.aimAngle);
    }

    private void PerformServerReallocation()
    {
        NetStatePayLoad serverState = _dataHandler.GetLastSeverStatePayLoad();
        NetStatePayLoad clientState = _dataHandler.GetStatePayLoadAtTick(serverState.tick);
        
        Vector3 serverPosition = serverState.position;
        Vector3 clientPosition = clientState.position;
        float positionError = Vector3.Distance(serverPosition, clientPosition);
        if (_dataHandler.canDodge == false)
        {
            _dataHandler.canDodge = serverState.canDodge;
        }
        
        if (positionError > 0.5)
        {
            transform.position = serverState.position;
            NetStatePayLoad[] stateBuffers = _dataHandler.GetStatePayLoads();
            stateBuffers[serverState.tick % CharacterDataHandler.NETWORK_BUFFER_SIZE] = serverState;

            int tickToProcess = serverState.tick + 1;
            while (tickToProcess < TickManager.Instance.GetTick())
            {
                NetInputPayLoad inputPayLoad = _dataHandler.GetInputPayloadAtTick(tickToProcess);
                ProcessMovement(inputPayLoad);
                stateBuffers[tickToProcess % CharacterDataHandler.NETWORK_BUFFER_SIZE] = new NetStatePayLoad()
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
        if (inputPayLoad.dodgePressed && _dataHandler.canDodge)
        {
            StartCoroutine(PerformDodge(inputPayLoad.direction));
        }
        
        Move(inputPayLoad.direction);
        Aim(inputPayLoad.aimAngle);
        
        _weaponComponent.UpdateComponent(inputPayLoad);
        
        UpdateAnimation(inputPayLoad);
    }

    private void Move(Vector3 direction)
    {
        if (_dataHandler.state == CharacterDataHandler.State.Dodge)
        {
            return;
        }

        _dataHandler.state = direction == Vector3.zero ? CharacterDataHandler.State.Idle : CharacterDataHandler.State.Move;
        
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        transform.position += direction * (tickManager.GetMinTickTime() * _dataHandler.speed.Value);
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
        if (_dataHandler.state == CharacterDataHandler.State.Dodge) yield break;

        _dataHandler.canDodge = false;
        
        float timer = 0;
        _dataHandler.state = CharacterDataHandler.State.Dodge;
        
        while (timer < _dataHandler.dodgeDuration.Value)
        {
            TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
            transform.position += direction.normalized * (tickManager.GetMinTickTime() * _dataHandler.dodgeSpeed.Value);
             
            yield return new WaitForSeconds(tickManager.GetMinTickTime());

            timer += tickManager.GetMinTickTime();
        }
        
        _dataHandler.state = CharacterDataHandler.State.Idle;

        yield return new WaitForSeconds(2);

        _dataHandler.canDodge = true;
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
    
    public CharacterDataHandler GetData()
    {
        return _dataHandler;
    }

    public WeaponComponent GetWeaponComponent()
    {
        return _weaponComponent;
    }

    public void TakeDamage(float damage)
    {
        if (IsServer)
        {
            _dataHandler.ReduceHealth(damage);
        }

        if (IsOwner)
        {
            _animator.PlayTakeDamage(false);
        }
    }
}
