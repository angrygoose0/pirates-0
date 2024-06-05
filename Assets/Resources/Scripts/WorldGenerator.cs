using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    public Tilemap seaTilemap;
    public TileBase seaTile;
    public int radius = 50;
    public float moveSpeed = 1f;

    private Vector3Int previousCenterTilePosition;
    private HashSet<Vector3Int> generatedTiles;

    void Start()
    {
        generatedTiles = new HashSet<Vector3Int>();
        GenerateInitialWorld();
    }

    void Update()
    {
        // Move the sea tilemap to simulate the ship's movement
        seaTilemap.transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        MaintainTileRadius();
    }

    void GenerateInitialWorld()
    {
        // Get the center position of the ship (ghost's position)
        Vector3 worldCenterPosition = transform.position;

        // Convert world position to tilemap position
        Vector3Int centerTilePosition = seaTilemap.WorldToCell(worldCenterPosition);
        previousCenterTilePosition = centerTilePosition;

        // Generate initial tiles in a radius around the center position
        GenerateTilesInRadius(centerTilePosition);
    }

    void GenerateTilesInRadius(Vector3Int centerTilePosition)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector3Int tilePosition = new Vector3Int(centerTilePosition.x + x, centerTilePosition.y + y, 0);
                float distance = Mathf.Sqrt(x * x + y * y);

                if (distance <= radius && !generatedTiles.Contains(tilePosition))
                {
                    seaTilemap.SetTile(tilePosition, seaTile);
                    generatedTiles.Add(tilePosition);
                }
            }
        }
    }

    void MaintainTileRadius()
    {
        // Get the current center tile position
        Vector3 worldCenterPosition = transform.position;
        Vector3Int currentCenterTilePosition = seaTilemap.WorldToCell(worldCenterPosition);

        // Only update if the center tile position has changed
        if (currentCenterTilePosition != previousCenterTilePosition)
        {
            previousCenterTilePosition = currentCenterTilePosition;

            // Generate new tiles in the new radius
            GenerateTilesInRadius(currentCenterTilePosition);

            // Remove tiles that are outside the radius
            List<Vector3Int> tilesToRemove = new List<Vector3Int>();

            foreach (var tilePosition in generatedTiles)
            {
                float distance = Vector3Int.Distance(currentCenterTilePosition, tilePosition);
                if (distance > radius)
                {
                    tilesToRemove.Add(tilePosition);
                }
            }

            foreach (var tilePosition in tilesToRemove)
            {
                seaTilemap.SetTile(tilePosition, null);
                generatedTiles.Remove(tilePosition);
            }
        }
    }
}
