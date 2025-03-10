
using Unity.Netcode;
using UnityEngine;

public class ServerController : NetController, ITickableEntity
{
    public override void Init()
    {
        player.playerData.Init();
        player.playerFeature.Init(player);
        player.weaponComponent.Init(player.arm, player.weaponHolder);
        player.weaponComponent.GiveDefaultWeapon();

        TickManager.Instance.AddEntity(this);
    }

    public void OnDestroy()
    {
        TickManager.Instance.RemoveEntity(this);
    }

    public void UpdateTick(int tick)
    {
        NetInputPayLoad[] processedInputPayload = player.inputProcessor.ProcessInputs();
        NetStatePayLoad[] processedStatePayload = new NetStatePayLoad[processedInputPayload.Length];
        for (int i = 0; i < processedInputPayload.Length; i++)
        {
            NetInputPayLoad inputPayload = processedInputPayload[i];
            player.playerFeature.ProcessFeature(inputPayload);

            NetStatePayLoad previousPayload = player.stateProcessor.LastProcessedState;
            NetStatePayLoad statePayLoad = new NetStatePayLoad()
            {
                inputSequence = inputPayload.payloadSequence,
                time = NetworkManager.Singleton.ServerTime.TimeAsFloat,
                tick = inputPayload.tick,
                position = transform.position,
                positionDelta = transform.position - previousPayload.position,
                aimAngle = inputPayload.aimAngle,
                shootPressed = inputPayload.shootPressed,
            };
            processedStatePayload[i] = player.stateProcessor.AddState(statePayLoad);
        }

        player.stateProcessor.SendProcessedStateClientRPC(processedStatePayload);
    }

    public override void OnRespawn()
    {
        base.OnRespawn();

        GameObject weaponPrefab = GameManager.Instance.GetSessionSettings().defaultWeapon;
        GameObject weaponObj = SpawnManager.Instance.SpawnObject(weaponPrefab, SpawnManager.SpawnType.NETWORK, Vector3.zero,
            Quaternion.identity);
        weaponObj.GetComponent<NetworkObject>().Spawn();
        Weapon weapon = weaponObj.GetComponent<Weapon>();
        GetComponent<WeaponComponent>().EquipWeapon(weapon);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

        player.playerData.state = PlayerData.State.Dead;
    }
}