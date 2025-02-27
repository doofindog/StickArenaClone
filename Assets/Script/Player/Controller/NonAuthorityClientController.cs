using System.Collections.Generic;
using UnityEngine;

public class NonAuthorityClientController : NetController
{
    private const float POSITION_ERROR_THRESHOLD = 0.05f;
    private List<NetStatePayLoad> fecthedStateCopy = new List<NetStatePayLoad>();

    public override void OnNetworkSpawn()
    {
        if (IsServer || IsOwner)
        {
            Destroy(this);
            return;
        }

        playerFeature.Init(this);
        weaponComponent.Init(playerComponent.arm, playerComponent.weaponHolder);
    }


    public void FixedUpdate()
    {
        SimulateInputs();
    }

    public void SimulateInputs()
    {
        NetStatePayLoad serverPayload = stateProcessor.GetServerState();
        playerFeature.ProcessFeature(serverPayload);
    }
}
