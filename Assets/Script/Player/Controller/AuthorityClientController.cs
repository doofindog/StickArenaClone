using Unity.Netcode;
using UnityEngine;


public class AuthorityClientController : NetController, ITickableEntity
{
	private const float POSITION_ERROR_THRESHOLD = 0.05f;

    public override void Init()
    {
        player.playerInput.Init(player);
        player.playerFeature.Init(player);
        player.weaponComponent.Init(player.arm, player.weaponHolder);

		CameraController.Instance.SetCameraTarget(transform);

        TickManager.Instance.AddEntity(this);
    }

    public void OnDestroy()
	{
		TickManager.Instance.RemoveEntity(this);
	}

	public void UpdateTick(int tick)
	{
		PerformServerReallocation();
    }

    public void FixedUpdate()
    {
        NetInputPayLoad inputPayLoad = player.inputProcessor.AddInput(new NetInputPayLoad()
        {
            time = NetworkManager.Singleton.LocalTime.TimeAsFloat,
            tick = TickManager.Instance.GetTick(),
            direction = player.playerData.direction,
            aimAngle = player.playerData.aimAngle,
			shootPressed = player.playerData.shootPressed,
        });

		PredictClientMovement(inputPayLoad);
    }

	private void PredictClientMovement(NetInputPayLoad pInputPayload)
	{
        player.playerFeature.ProcessFeature(pInputPayload);

        //Client Side Prediction
        player.stateProcessor.AddState(new NetStatePayLoad()
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
        if (player.stateProcessor == null || player.inputProcessor == null)
        {
            Debug.LogWarning("NetStateProcessor or NetInputProcessor is not assigned.");
            return;
        }

        NetStatePayLoad serverState = player.stateProcessor.GetServerState();
		NetStatePayLoad clientState = player.stateProcessor.GetStateAtSequenceNumber(serverState.inputSequence);
		
		float positionDifference = Vector3.Distance(serverState.position, clientState.position);
		if (!(positionDifference > POSITION_ERROR_THRESHOLD))
		{
			return;
		}

		transform.position = serverState.position;
		player.stateProcessor.UpdateState(serverState);

		int sequenceToProcess = serverState.inputSequence + 1;
		int lastSequenceCount = player.inputProcessor.GetCurrentSequenceCount() - 1;
		while (sequenceToProcess < lastSequenceCount)
		{
			NetInputPayLoad inputPayLoad = player.inputProcessor.GetPayloadAtSequence(sequenceToProcess);
			if(inputPayLoad.isNull)
			{
				continue;
			}

            player.playerFeature.ProcessFeature(inputPayLoad);

			NetStatePayLoad netStatePayLoad = new NetStatePayLoad()
			{
				inputSequence = sequenceToProcess,
				tick = inputPayLoad.tick,
				position = transform.position,
				aimAngle = inputPayLoad.aimAngle,
			};

            player.stateProcessor.UpdateAtSequence(sequenceToProcess, netStatePayLoad);
			sequenceToProcess++;
		}
	}
}
