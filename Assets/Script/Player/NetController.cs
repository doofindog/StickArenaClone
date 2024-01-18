using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetController : NetworkBehaviour
{
    protected CharacterDataHandler dataHandler;
    protected PlayerInputHandler playerInputHandler;
    protected WeaponComponent weaponComponent;
    protected CharacterAnimator animator;
    
    public virtual void Awake()
    {
        dataHandler = GetComponent<CharacterDataHandler>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
        weaponComponent = GetComponent<WeaponComponent>();
        animator = GetComponent<CharacterAnimator>();
    }
    
    public virtual void Start()
    {
        dataHandler.Init();
    }
    
    
    protected virtual void ProcessMovement(NetInputPayLoad inputPayLoad)
    {
        if (inputPayLoad.dodgePressed && dataHandler.canDodge)
        {
            StartCoroutine(PerformDodge(inputPayLoad.direction));
        }
        
        Move(inputPayLoad.direction);
        Flip(inputPayLoad.aimAngle);
        
        weaponComponent.UpdateComponent(inputPayLoad);
        
        UpdateAnimation(inputPayLoad);
    }

    protected virtual void Move(Vector3 direction)
    {
        if (dataHandler.state == CharacterDataHandler.State.Dodge)
        {
            return;
        }

        dataHandler.state = direction == Vector3.zero ? CharacterDataHandler.State.Idle : CharacterDataHandler.State.Move;
        
        TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
        transform.position += direction * (tickManager.GetMinTickTime() * dataHandler.speed.Value);
    }

    protected virtual void Flip(float aimAngle)
    {
        SpriteRenderer playerSprite = GetComponentInChildren<SpriteRenderer>();
        bool isFlip = aimAngle is > 90 and < 270;
        playerSprite.flipX = isFlip;
    }
    
    protected virtual IEnumerator PerformDodge(Vector3 direction)
    {
        if (dataHandler.state == CharacterDataHandler.State.Dodge) yield break;

        dataHandler.canDodge = false;
        
        float timer = 0;
        dataHandler.state = CharacterDataHandler.State.Dodge;
        
        while (timer < dataHandler.dodgeDuration.Value)
        {
            TickManager tickManager = GameNetworkManager.Instance.GetTickManager();
            transform.position += direction.normalized * (tickManager.GetMinTickTime() * dataHandler.dodgeSpeed.Value);
             
            yield return new WaitForSeconds(tickManager.GetMinTickTime());

            timer += tickManager.GetMinTickTime();
        }
        
        dataHandler.state = CharacterDataHandler.State.Idle;

        yield return new WaitForSeconds(2);

        dataHandler.canDodge = true;
    }
    
    protected virtual void UpdateAnimation(NetInputPayLoad inputPayLoad)
    {
        if (!IsClient || !IsOwner) return;
        
        if (inputPayLoad.direction != Vector3.zero)
        {
            animator.PlayWalk();
        }
        else
        {
            animator.PlayIdle();
        }
    }
    public virtual void TakeDamage(float damage)
    {
        
    }
    
}
