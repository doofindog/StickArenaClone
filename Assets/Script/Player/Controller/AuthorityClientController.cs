using Unity.Netcode;
using UnityEngine;


public class AuthorityClientController : NetController, ITickableEntity
{
	private const float POSITION_ERROR_THRESHOLD = 0.05f;
	
	public override void OnNetworkSpawn()
	{
		if(IsServer || !IsOwner)
		{
			Destroy(this);
			return;
		}

		playerInput.Init(this);
		playerFeature.Init(this);
		weaponComponent.Init(playerComponent.arm, playerComponent.weaponHolder);
		TickManager.Instance.AddEntity(this);
        LocalPlayerEvents.SendLocalPlayerSpawned(this.gameObject);
    }

	public override void OnDestroy()
	{
		base.OnDestroy();

		TickManager.Instance.RemoveEntity(this);
	}

	public void UpdateTick(int tick)
	{
		PerformServerReallocation();
    }

    public void FixedUpdate()
    {
        NetInputPayLoad inputPayLoad = inputProcessor.AddInput(new NetInputPayLoad()
        {
            time = NetworkManager.Singleton.ServerTime.TimeAsFloat,
            tick = TickManager.Instance.GetTick(),
            direction = playerData.direction,
            aimAngle = playerData.aimAngle,
			shootPressed = playerData.shootPressed,
        });

		PredictClientMovement(inputPayLoad);
    }

	private void PredictClientMovement(NetInputPayLoad pInputPayload)
	{
		playerFeature.ProcessFeature(pInputPayload);

        //Client Side Prediction
        stateProcessor.AddState(new NetStatePayLoad()
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
        if (stateProcessor == null || inputProcessor == null)
        {
            Debug.LogWarning("NetStateProcessor or NetInputProcessor is not assigned.");
            return;
        }

        NetStatePayLoad serverState = stateProcessor.GetServerState();
		NetStatePayLoad clientState = stateProcessor.GetStateAtSequenceNumber(serverState.inputSequence);
		
		float positionDifference = Vector3.Distance(serverState.position, clientState.position);
		if (!(positionDifference > POSITION_ERROR_THRESHOLD))
		{
			return;
		}

		transform.position = serverState.position;
		stateProcessor.UpdateState(serverState);

		int sequenceToProcess = serverState.inputSequence + 1;
		int lastSequenceCount = inputProcessor.GetCurrentSequenceCount() - 1;
		while (sequenceToProcess < lastSequenceCount)
		{
			NetInputPayLoad inputPayLoad = inputProcessor.GetPayloadAtSequence(sequenceToProcess);
			if(inputPayLoad.isNull)
			{
				continue;
			}

			playerFeature.ProcessFeature(inputPayLoad);

			NetStatePayLoad netStatePayLoad = new NetStatePayLoad()
			{
				inputSequence = sequenceToProcess,
				tick = inputPayLoad.tick,
				position = transform.position,
				aimAngle = inputPayLoad.aimAngle,
			};

			stateProcessor.UpdateAtSequence(sequenceToProcess, netStatePayLoad);
			sequenceToProcess++;
		}
	}
}
