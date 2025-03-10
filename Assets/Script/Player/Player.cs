using System;
using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(PlayerData))]
[RequireComponent(typeof(WeaponComponent))]
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerFeatureController))]
[RequireComponent(typeof(NetInputProcessor), typeof(NetStateProcessor))]
public class Player : NetworkBehaviour, IDamageableEntity
{
    [Header("Componenets")]
    public Transform arm;
    public Transform weaponHolder;
    public SpriteRenderer spriteRenderer;

    [Header("Player Data")]
    public PlayerData playerData;
    public PlayerInputHandler playerInput;
    public WeaponComponent weaponComponent;
    public NetInputProcessor inputProcessor;
    public NetStateProcessor stateProcessor;
    public PlayerFeatureController playerFeature;
    public PlayerAnimationController playerAnimController;
    public NetController netController;

    public Vector3 gizmoPosition;
    public Action<HitResponseData> TakeDamageEvent { get; set; }

    public void Awake()
    {
        playerData = GetComponent<PlayerData>();
        playerInput = GetComponent<PlayerInputHandler>();
        weaponComponent = GetComponent<WeaponComponent>();
        inputProcessor = GetComponent<NetInputProcessor>();
        stateProcessor = GetComponent<NetStateProcessor>();
        playerFeature = GetComponent<PlayerFeatureController>();
        playerAnimController = GetComponent<PlayerAnimationController>();
    }

    public void Start()
    {
        void HandleServer()
        {
            netController = gameObject.AddComponent<ServerController>();
        }

        void HandleClient()
        {
            if(NetworkObject.IsLocalPlayer)
            {
                PlayerEvents.SendPlayerSpawned(this.gameObject);
                netController = gameObject.AddComponent<AuthorityClientController>();
            }
            else
            {
                netController = gameObject.AddComponent<NonAuthorityClientController>();
            }
        }

        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);

        netController.Init();
    }

    public void TakeDamage(HitResponseData hitResponseData)
    {
        TakeDamageEvent?.Invoke(hitResponseData);
        TakeDamageServerRPC(hitResponseData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRPC(HitResponseData hitResponseData)
    {
        TakeDamageEvent?.Invoke(hitResponseData);
    }
}
