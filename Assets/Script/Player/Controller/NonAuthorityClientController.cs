using System.Collections.Generic;
using UnityEngine;

public class NonAuthorityClientController : NetController
{
    private const float POSITION_ERROR_THRESHOLD = 0.05f;
    private List<NetStatePayLoad> fecthedStateCopy = new List<NetStatePayLoad>();

    public override void Init()
    {
        player.playerFeature.Init(player);
        player.weaponComponent.Init(player.arm, player.weaponHolder);
    }


    public void FixedUpdate()
    {
        SimulateInputs();
    }

    public void SimulateInputs()
    {
        NetStatePayLoad serverPayload = player.stateProcessor.GetServerState();
        player.playerFeature.ProcessFeature(serverPayload);
    }
}
