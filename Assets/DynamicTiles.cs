using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DynamicTiles : NetworkBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Vector2 tileSize;
    [SerializeField] private Vector2 offsetPosition;

    private List<GameObject> tiles = new List<GameObject>();
    
    public void Awake()
    {
        GenerateTile();
    }

    public void Start()
    {
        StartCoroutine(UpdateTiles());
    }

    private IEnumerator UpdateTiles()
    {
        yield return new WaitForSeconds(10);
        
        while (true)
        {
            System.Random random = new System.Random();
            List<GameObject> tilesToOpen = this.tiles.OrderBy(tile => random.Next()).Take(10).ToList();

            foreach (GameObject tile in tilesToOpen)
            {
                Animator anim = tile.GetComponent<Animator>();
                anim.Play("GroundOpen");
            }

            yield return new WaitForSeconds(10);
            
            foreach (GameObject tile in tilesToOpen)
            {
                Animator anim = tile.GetComponent<Animator>();
                anim.Play("GroundClose");
            }
            
            yield return new WaitForSeconds(3);
        }
    }

    private void GenerateTile()
    {
        Vector2 startPosition = new Vector2(tileSize.x, tileSize.y) * -0.5f;
        Vector2 endPosition = new Vector2(tileSize.x, tileSize.y) * 0.5f;
        
        for (float y = startPosition.y; y < endPosition.y; y++)
        {
            for (float x = startPosition.x; x < endPosition.x; x++)
            {
                Vector2 tilePosition = new Vector2(x, y);
                GameObject ground = Instantiate(tilePrefab, tilePosition + offsetPosition, Quaternion.identity);
                tiles.Add(ground);
            }
        }
    }
}
