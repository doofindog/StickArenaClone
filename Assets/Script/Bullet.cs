using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour, ITickableEntity
{
    private const float BULLET_LIFE = 3.0f;

    [SerializeField] private GameObject impactParticle;

    private int _id;
    private bool _isEnabled;
    private bool _hasHitObstacle;
    private int m_damage;
    private float m_life;
    private float m_speed;
    private float m_rtt;
    private float m_inputDelay;
    private Vector3 m_startPosition;
    private NetworkVariable<ulong> m_playerNetID = new NetworkVariable<ulong>();

    public override void OnNetworkDespawn()
    {
        void HandleServer()
        {
            TickManager.Instance.RemoveEntity(this);
        }

        void HandleClient() { }

        base.OnNetworkDespawn();
        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
    }

    public void Awake()
    {
        void HandleServer()
        {
            TickManager.Instance.AddEntity(this);
        }

        void HandleClient() { }

        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
    }

    public void OnEnable()
    {
        _isEnabled = true;
        m_life = 0;
    }

    public void OnDisable()
    {
        _isEnabled = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        TickManager.Instance.RemoveEntity(this);
    }

    public void Initialise(ulong playerID, int damage, float bulletSpeed, float time = 0)
    {
        m_playerNetID.Value = playerID;
        m_speed = bulletSpeed;
        m_rtt = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(playerID);
        m_inputDelay = time - NetworkManager.Singleton.ServerTime.TimeAsFloat;
        m_damage = damage;
        m_startPosition = transform.position;
    }

    public void FixedUpdate()
    {
        transform.position += transform.right * (m_speed * Time.fixedDeltaTime);
        m_life += Time.fixedDeltaTime;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (NetworkManager.LocalClientId == m_playerNetID.Value && (other.gameObject.TryGetComponent(out IDamageableEntity damageableEntity)))
        {
            Debug.Log("bullet collided with player");
            HitResponseData hitResponseData = new HitResponseData()
            {
                hitTime = NetworkManager.Singleton.ServerTime.TimeAsFloat,
                hitVelocity = m_speed,
                traceStart = m_startPosition,
                projectileDirection = (transform.position + transform.right) - transform.position,
                damage = m_damage,
            };

            damageableEntity.TakeDamage(hitResponseData);
        }

        HandleImpact();
    }

    private void HandleImpact()
    {
        GameObject particle = ObjectPool.Instance.GetPooledObject(impactParticle, transform.position, quaternion.identity).gameObject;
        particle.SetActive(true);

        gameObject.SetActive(false);
        transform.position = new Vector3(-1000, -1000, -1000);
    }

    public void UpdateTick(int tick)
    {
        //throw new NotImplementedException();
    }
}