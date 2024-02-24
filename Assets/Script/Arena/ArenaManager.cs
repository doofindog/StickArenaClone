using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ArenaManager : NetworkBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector2 tileSize;
    [SerializeField] private Vector2 offsetPosition;
    
    private IArenaState _currentState;
    private Dictionary<Vector2, GameObject> _dynamicTiles = new Dictionary<Vector2, GameObject>();
    
    public void Awake()
    {
        IArenaState[] components = GetComponents<IArenaState>();
        foreach (IArenaState state in components)
        {
            state.Init(this);
        }
        
        ChangeState(GetComponent<ArenaNormalState>());
        GenerateTile();
    }

    private void ChangeState(IArenaState state)
    {
        _currentState?.OnExitState();
        _currentState = state;
        _currentState?.OnEnterState();
    }

    public void Update()
    {
        _currentState?.OnUpdateState();
    }
    

    private void GenerateTile()
    {
        Vector2 startPosition = new Vector2(tileSize.x, tileSize.y) * -0.5f;
        
        for (int y = 0; y < tileSize.y; y++)
        {
            for (int x = 0; x < tileSize.x; x++)
            {
                Vector2 tilePosition = new Vector2(startPosition.x + x, startPosition.y + y);
                GameObject ground = Instantiate(tilePrefab, tilePosition + offsetPosition, Quaternion.identity);
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
}
