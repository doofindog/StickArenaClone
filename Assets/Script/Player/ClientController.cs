using Unity.Netcode;
using UnityEngine;

public class ClientController : NetController, ITickableEntity, IDamageableEntity
{
	private const float POSITION_ERROR_THRESHOLD = 0.5f;
	
	private NetInputProcessor _netInputProcessor;
	private NetStateProcessor _netStateProcessor;
	

	public override void Awake()
	{
		base.Awake();

		_netInputProcessor = GetComponent<NetInputProcessor>();
		_netStateProcessor = GetComponent<NetStateProcessor>();
	}

	public override void Start()
	{
		base.Start();

		if (IsOwner)
		{
			PlayerInputHandler.Init(this);
		}
	}

	public override void OnNetworkSpawn()
	{
		if ((IsServer && !IsHost) || (IsHost && !IsOwner))
		{
			Destroy(this);
		}
		
		TickManager.Instance.AddEntity(this);
		if (IsOwner)
		{
			PlayerEvents.SendPlayerSpawned(gameObject);
		}

		NetworkObject networkObject = GetComponent<NetworkObject>();
		GameEvents.SendPlayerSpawned(networkObject.OwnerClientId, networkObject);
	}

	public void UpdateTick(int tick)
	{
		if (IsEnabled == false) return; 
		
		if (!IsLocalPlayer)
		{
			SimulateMovement();
		}
		else
		{
			PerformServerReallocation();
			
			NetInputPayLoad inputPayLoad = DataHandler.GetNewInputPayLoad();
			_netInputProcessor.AddInput(inputPayLoad);
			
			ProcessMovement(inputPayLoad);

			_netStateProcessor.AddState(new NetStatePayLoad()
			{
				tick = inputPayLoad.tick,
				position = transform.position,
				aimAngle = inputPayLoad.aimAngle,
				dodge = inputPayLoad.dodgePressed,
				canDodge = DataHandler.canDodge
			});
		}
	}

	private void PerformServerReallocation()
	{
		NetStatePayLoad serverState = _netStateProcessor.GetLastProcessedState();
		NetStatePayLoad clientState = _netStateProcessor.GetStateAtTick(serverState.tick);
		
		if (DataHandler.canDodge == false)
		{
			DataHandler.canDodge = DataHandler.canDodge;
		}
		
		Vector3 serverPosition = serverState.position;
		Vector3 clientPosition = clientState.position;

		float positionError = Vector3.Distance(serverPosition, clientPosition);
		if (positionError > POSITION_ERROR_THRESHOLD)
		{
			transform.position = serverState.position;
			_netStateProcessor.UpdateState(serverState);

			int tickToProcess = serverState.tick + 1;
			while (tickToProcess < TickManager.Instance.GetTick())
			{
				NetInputPayLoad inputPayLoad = _netInputProcessor.GetPayloadAtTick(tickToProcess);
				ProcessMovement(inputPayLoad);
				NetStatePayLoad netStatePayLoad = new NetStatePayLoad()
				{
					tick = inputPayLoad.tick,
					position = transform.position,
					aimAngle = inputPayLoad.aimAngle,
					dodge = inputPayLoad.dodgePressed,
				};
				
				_netStateProcessor.UpdateStateAtToTick(tickToProcess, netStatePayLoad);
				tickToProcess++;
			}
		}
	}

	public override void OnDestroy()
	{
		TickManager.Instance.RemoveEntity(this);
	}
	
	protected virtual void SimulateMovement()
	{
		NetStatePayLoad latestServerState = _netStateProcessor.GetLastProcessedState();
		transform.position = latestServerState.position;

		WeaponComponent.Aim(latestServerState.aimAngle);
		
		Flip(latestServerState.aimAngle);
	}
	
	public override void TakeDamage(float damage, NetworkObject source)
	{
		if (IsOwner)
		{
			if(DataHandler.state == CharacterDataHandler.State.Dead) return;
			
			Animator.PlayTakeDamage(false);
		}
	}

	public override void Die()
	{
		base.Die();
		DataHandler.state = CharacterDataHandler.State.Dead;
		IsEnabled = false;
		Animator.PlayDeathAnimation(true);
		
		if (IsLocalPlayer)
		{
			PlayerEvents.SendPlayerDied();
		}
	}
}
