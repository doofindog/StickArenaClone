using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ArenaManager : NetworkBehaviour, ITickableEntity
{
    public enum States
    {
        Idle,
        Escapist,
    }
    
    public static ArenaManager Instance { get; private set; }
    
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private Vector2 tileSize;
    [SerializeField] private Vector2 offsetPosition;
    
    private IArenaState _currentState;
    private Dictionary<Vector2, GameObject> _dynamicTiles = new Dictionary<Vector2, GameObject>();
    private Dictionary<States, IArenaState> _arenaStates = new Dictionary<States, IArenaState>();

    public void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Start()
    {
        TickManager.Instance.AddEntity(this);
    }

    public void Init()
    {
        IArenaState[] components = GetComponents<IArenaState>();
        foreach (IArenaState state in components)
        {
            state.Init(this);
        }
        
        _arenaStates.Add(States.Idle, GetComponent<ArenaNormalState>());
        
        GenerateTile();
    }

    public void ChangeState(States arenaState)
    {
        _currentState?.OnExitState();
        _currentState = _arenaStates[arenaState];
        _currentState?.OnEnterState();
    }

    private void GenerateTile()
    {
        GameObject dynamicTiles = new GameObject("Dynamic Tiles");
        dynamicTiles.transform.position = Vector3.zero;
        Vector2 startPosition = new Vector2(tileSize.x, tileSize.y) * -0.5f;
        
        for (int y = 0; y < tileSize.y; y++)
        {
            for (int x = 0; x < tileSize.x; x++)
            {
                Vector2 tilePosition = new Vector2(startPosition.x + x, startPosition.y + y);
                GameObject ground = Instantiate(tilePrefab, tilePosition + offsetPosition, Quaternion.identity);
                ground.transform.SetParent(dynamicTiles.transform);
                _dynamicTiles.Add(new Vector2(x, y), ground);
            }
        }
    }

    public List<GameObject> GetAllDynamicTiles()
    {
        return _dynamicTiles.Values.ToList();
    }

    public DynamicTiles GetTile(int x, int y)
    {
        _dynamicTiles.TryGetValue(new Vector2(x, y), out GameObject tile);
        return tile != null ? tile.GetComponent<DynamicTiles>() : null;
    }

    public void UpdateTick(int tick)
    {
        _currentState?.OnUpdateState();
    }
}
