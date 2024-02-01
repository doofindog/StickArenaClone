using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class NetController : NetworkBehaviour
{
    protected bool IsEnabled;
    protected CharacterDataHandler DataHandler;
    protected PlayerInputHandler PlayerInputHandler;
    protected WeaponComponent WeaponComponent;
    protected CharacterAnimator Animator;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private CharacterAnimator _animator;
    
    public virtual void Awake()
    {
        DataHandler = GetComponent<CharacterDataHandler>();
        PlayerInputHandler = GetComponent<PlayerInputHandler>();
        WeaponComponent = GetComponent<WeaponComponent>();
        Animator = GetComponent<CharacterAnimator>();
        IsEnabled = true;
    }

    public override void OnNetworkSpawn()
    {
        ulong clientID = GetComponent<NetworkObject>().OwnerClientId;
        Team team = SessionTeamManager.Instance.GetPlayerTeam(clientID);
        _spriteRenderer.material.SetColor("_newColour", team.color);
    }
    
    public virtual void Start()
    {
        DataHandler.Init();
    }
    
    protected virtual void ProcessMovement(NetInputPayLoad inputPayLoad)
    {
        if (inputPayLoad.dodgePressed && DataHandler.canDodge)
        {
            StartCoroutine(PerformDodge(inputPayLoad.direction));
        }
        
        Move(inputPayLoad.direction);
        Flip(inputPayLoad.aimAngle);
        
        WeaponComponent.UpdateComponent(inputPayLoad);
        
        UpdateAnimation(inputPayLoad);
    }

    protected virtual void Move(Vector3 direction)
    {
        if (DataHandler.state == CharacterDataHandler.State.Dodge)
        {
            return;
        }
        
        DataHandler.state = direction == Vector3.zero ? CharacterDataHandler.State.Idle : CharacterDataHandler.State.Move;
        
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        transform.position += direction * (tickManager.GetMinTickTime() * DataHandler.speed.Value);
    }

    protected virtual void Flip(float aimAngle)
    {
        SpriteRenderer playerSprite = GetComponentInChildren<SpriteRenderer>();
        bool isFlip = aimAngle is > 90 and < 270;
        playerSprite.flipX = isFlip;
    }
    
    protected virtual IEnumerator PerformDodge(Vector3 direction)
    {
        if (DataHandler.state == CharacterDataHandler.State.Dodge) yield break;

        DataHandler.canDodge = false;
        
        float timer = 0;
        DataHandler.state = CharacterDataHandler.State.Dodge;
        
        while (timer < DataHandler.dodgeDuration.Value)
        {
            TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
            transform.position += direction.normalized * (tickManager.GetMinTickTime() * DataHandler.dodgeSpeed.Value);
             
            yield return new WaitForSeconds(tickManager.GetMinTickTime());

            timer += tickManager.GetMinTickTime();
        }
        
        DataHandler.state = CharacterDataHandler.State.Idle;

        yield return new WaitForSeconds(1.5f);

        DataHandler.canDodge = true;
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
    public virtual void TakeDamage(float damage)
    {
        
    }

    public virtual void Die()
    {
        
    }
    
    public virtual void Respawn()
    {
        IsEnabled = true;
        Animator.PlayIdle(false);
        gameObject.SetActive(true);
        DataHandler.Refresh();
        GetComponent<Collider2D>().enabled = true;
    }

    public SpriteRenderer GetSpriteRendered()
    {
        return _spriteRenderer;
    }
}
