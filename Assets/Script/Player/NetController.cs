using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetController : NetworkBehaviour
{
    protected bool IsEnabled;

    [Header("Components")]
    public CharacterDataHandler DataHandler;
    public PlayerInputHandler PlayerInputHandler;
    public WeaponComponent WeaponComponent;
    public CharacterAnimator Animator;
    public SpriteRenderer CharacterSprite;
    public Transform Arm;

    [SerializeField] protected bool interpolate;
    [SerializeField] private CharacterAnimator _animator;
    [SerializeField] private Transform _crownPlaceholder;

    protected List<PlayerFeature> playerFeature = new List<PlayerFeature>();

    public virtual void Awake()
    {
        GameEvents.OnGameOverEvent += StopControls;
        
        DataHandler = GetComponent<CharacterDataHandler>();
        PlayerInputHandler = GetComponent<PlayerInputHandler>();
        WeaponComponent = GetComponent<WeaponComponent>();
        Animator = GetComponent<CharacterAnimator>();
        Arm = transform.Find("Arm");

        IsEnabled = true;
        
        playerFeature.Add(new Movement(this));
        playerFeature.Add(new Flip(this));
        playerFeature.Add(new Aim(this));
    }

    public override void OnNetworkSpawn()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        ulong clientID = netObj.OwnerClientId;
        Team team = TeamManager.Instance.GetTeamFromID(clientID);
        CharacterSprite.material.SetColor("_newColour", team.color);
    }
    
    public virtual void Start()
    {
        DataHandler.Init();
    }

    protected void ProcessFeature(NetInputPayLoad pInputPayload)
    {
        foreach (PlayerFeature feature in playerFeature)
        {
            feature.Process(pInputPayload);
        }
    }

    protected virtual IEnumerator PerformDodge(Vector3 direction)
    {
        if (DataHandler.state == CharacterDataHandler.State.Dodge) yield break;

        GetComponent<Collider2D>().enabled = false;
        DataHandler.canDodge.Anticipate(false);
        DataHandler.state = CharacterDataHandler.State.Dodge;

        TickManager tickManager = TickManager.Instance;
        float tickInterval = tickManager.GetMinTickTime();
        int dodgeTicks = Mathf.CeilToInt(DataHandler.dodgeDuration.Value / tickInterval);

        Vector3 startPos = transform.position;
        Vector3 totalDisplacement = direction.normalized * (DataHandler.dodgeSpeed.Value * DataHandler.dodgeDuration.Value);
        Vector3 targetPos = startPos + totalDisplacement;

        for (int i = 0; i < dodgeTicks; i++)
        {
            float t = (i + 1) / (float)dodgeTicks;
            // Lerp from start to target position
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return new WaitForSeconds(tickInterval);
        }

        transform.position = targetPos;

        GetComponent<Collider2D>().enabled = true;
        DataHandler.state = CharacterDataHandler.State.Idle;

        int waitTicks = Mathf.CeilToInt(1.5f / tickInterval);
        for (int i = 0; i < waitTicks; i++)
        {
            yield return new WaitForSeconds(tickInterval);
        }

        DataHandler.canDodge.Anticipate(true);
        DataHandler.state = CharacterDataHandler.State.Idle;
    }

    protected virtual void Dodge()
    {

    }
    
    protected virtual void UpdateAnimation(NetInputPayLoad inputPayLoad)
    {
        if (!IsClient || !IsOwner) return;
        
        if(DataHandler.state == CharacterDataHandler.State.Dead) return;
        
        if (inputPayLoad.direction != Vector3.zero)
        {
            Animator.PlayWalk();
        }
        else
        {
            Animator.PlayIdle(true);
        }
    }
    public virtual void TakeDamage(HitResponseData hitResponseData)
    {
        
    }

    public virtual void OnDespawn()
    {
        GetComponent<Collider2D>().enabled = false;
    }
    
    public virtual void OnRespawn()
    {
        IsEnabled = true;
        gameObject.SetActive(true);
        Animator.PlayIdle(false);
        DataHandler.Reset();
        GetComponent<Collider2D>().enabled = true;
    }

    public virtual void Die()
    {
        
    }

    public virtual void Drown()
    {

    }

    public Transform GetCrownPlaceholder()
    {
        return _crownPlaceholder;
    }

    private void StopControls()
    {
        IsEnabled = false;
    }
}
