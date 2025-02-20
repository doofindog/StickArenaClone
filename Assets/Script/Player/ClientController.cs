using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

public class ClientController : NetController, ITickableEntity, IDamageableEntity
{
	private const float POSITION_ERROR_THRESHOLD = 0.1f;
	
	private NetInputProcessor m_netInputProcessor;
	private NetStateProcessor m_netStateProcessor;
	

	public override void Awake()
	{
		base.Awake();

		m_netInputProcessor = GetComponent<NetInputProcessor>();
		m_netStateProcessor = GetComponent<NetStateProcessor>();
	}
	
	public override void OnNetworkSpawn()
	{
		if ((IsServer && !IsHost) || (IsHost && !IsOwner))
		{
			Destroy(this);
		}
		
		TickManager.Instance.AddEntity(this);
		
		NetworkObject networkObject = GetComponent<NetworkObject>();
		GameEvents.SendPlayerSpawned(networkObject.OwnerClientId, networkObject);
	}

    public override void Start()
	{
		base.Start();

		if (IsOwner)
		{
			PlayerInputHandler.Init(this);
		}
		
		if (IsOwner)
		{
			PlayerEvents.SendPlayerSpawned(gameObject);
		}
	}



	public void UpdateTick(int tick)
	{
		if (IsEnabled == false) return;

		//PerformServerReallocation();
    }

    public void Update()
    {
        NetInputPayLoad inputPayLoad = m_netInputProcessor.AddInput(new NetInputPayLoad()
        {
            time = NetworkManager.Singleton.ServerTime.TimeAsFloat,
            tick = TickManager.Instance.GetTick(),
            direction = DataHandler.direction,
            aimAngle = DataHandler.aimAngle,
        });

		PredictClientMovement(inputPayLoad);
    }

	private void PredictClientMovement(NetInputPayLoad pInputPayload)
	{
        ProcessFeature(pInputPayload);

        //Client Side Prediction
        m_netStateProcessor.AddState(new NetStatePayLoad()
        {
			inputSequence = pInputPayload.payloadSequence,
            time = NetworkManager.Singleton.ServerTime.TimeAsFloat,
            tick = pInputPayload.tick,
            position = transform.position,
            aimAngle = pInputPayload.aimAngle,
        });
    }

    private void PerformServerReallocation()
	{
        if (m_netStateProcessor == null || m_netInputProcessor == null)
        {
            Debug.LogWarning("NetStateProcessor or NetInputProcessor is not assigned.");
            return;
        }

        NetStatePayLoad serverState = m_netStateProcessor.GetLastProcessedState();
		NetStatePayLoad clientState = m_netStateProcessor.GetStateAtSequenceNumber(serverState.tick);
		
		float positionError = Vector3.Distance(serverState.position, clientState.position);

		if (!(positionError > POSITION_ERROR_THRESHOLD))
		{
			return;
		}

		transform.position = serverState.position;
		m_netStateProcessor.UpdateState(serverState);

		int tickToProcess = serverState.tick + 1;
		while (tickToProcess < TickManager.Instance.GetTick())
		{
			NetInputPayLoad inputPayLoad = m_netInputProcessor.GetPayloadAtSequence(tickToProcess);

			ProcessFeature(inputPayLoad);

			NetStatePayLoad netStatePayLoad = new NetStatePayLoad()
			{
				tick = inputPayLoad.tick,
				position = transform.position,
				aimAngle = inputPayLoad.aimAngle,
			};

			m_netStateProcessor.UpdateStateAtToTick(tickToProcess, netStatePayLoad);
			tickToProcess++;
		}
	}

	public override void OnDestroy()
	{
		GameManager.Instance.tickManager.RemoveEntity(this);
	}
	
	protected virtual void SimulateMovement()
	{
		NetStatePayLoad latestServerState = m_netStateProcessor.GetLastProcessedState();
		transform.position = Vector3.Lerp(transform.position, latestServerState.position, 0.5f);

		//if (latestServerState.firedWeapon)
		//{
		//	WeaponComponent.TriggerWeapon(new Weapon.Params()
		//	{
		//		tick = latestServerState.tick
		//	});
		//}
		//else
		//{
		//	WeaponComponent.ReleaseTrigger();
		//}
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
