using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetController : NetworkBehaviour
{
    protected bool IsEnabled;
    protected CharacterDataHandler DataHandler;
    protected PlayerInputHandler PlayerInputHandler;
    protected WeaponComponent WeaponComponent;
    protected CharacterAnimator Animator;
    
    public virtual void Awake()
    {
        DataHandler = GetComponent<CharacterDataHandler>();
        PlayerInputHandler = GetComponent<PlayerInputHandler>();
        WeaponComponent = GetComponent<WeaponComponent>();
        Animator = GetComponent<CharacterAnimator>();
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

        yield return new WaitForSeconds(2);

        DataHandler.canDodge = true;
    }
    
    protected virtual void UpdateAnimation(NetInputPayLoad inputPayLoad)
    {
        if (!IsClient || !IsOwner) return;
        
        if (inputPayLoad.direction != Vector3.zero)
        {
            Animator.PlayWalk();
        }
        else
        {
            Animator.PlayIdle();
        }
    }
    public virtual void TakeDamage(float damage)
    {
        
    }
    
}
