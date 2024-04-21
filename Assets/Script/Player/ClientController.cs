using Unity.Netcode;
using UnityEngine;

public class ClientController : NetController, ITickableEntity, IDamageableEntity
{
	private const float POSITION_ERROR_THRESHOLD = 0.2f;
	
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
				canDodge = DataHandler.canDodge,
				firedWeapon = inputPayLoad.attackPressed
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
					firedWeapon = inputPayLoad.attackPressed
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
		Debugger.Log("[WEAPON] Weapon Fired State : " + latestServerState.firedWeapon);
		if (latestServerState.firedWeapon)
		{
			WeaponComponent.TriggerWeapon(new Weapon.Params()
			{
				tick = latestServerState.tick
			});
		}
		else
		{
			WeaponComponent.ReleaseTrigger();
		}

		Flip(latestServerState.aimAngle);
	}
	
	public override void TakeDamage(HitResponseData hitResponseData)
	{
		if (IsOwner)
		{
			if(DataHandler.state == CharacterDataHandler.State.Dead) return;
			
			Animator.PlayTakeDamage(false);
		}
	}

	public override void Die()
	{
		
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		DataHandler.state = CharacterDataHandler.State.Dead;
		IsEnabled = false;
		
		GameEvents.SendPlayerKilledEvent(OwnerClientId);
		if (IsLocalPlayer)
		{
			PlayerEvents.SendPlayerDied();
		}
	}

	public override void Drown()
	{
		Animator.PlayDeathAnimation(true);
	}
}
