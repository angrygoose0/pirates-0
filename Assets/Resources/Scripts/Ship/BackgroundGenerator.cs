using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BackgroundGenerator : MonoBehaviour
{
    public GameObject anchorObject;      // Reference to the anchor GameObject
    public Tilemap tilemap;              // Reference to the Tilemap component
    public Tile tile;                    // Reference to the Tile asset to place
    public int radius = 10;              // Radius around the anchor within which tiles will be generated

    private HashSet<Vector3Int> activeTiles = new HashSet<Vector3Int>(); // Tracks currently active tiles
    private Vector3Int lastAnchorPos;

    void Start()
    {
        if (anchorObject == null || tilemap == null || tile == null)
        {
            Debug.LogError("Anchor Object, Tilemap, or Tile is not assigned.");
            return;
        }

        lastAnchorPos = GetAnchorTilePosition();
        UpdateTilemap();
    }

    void Update()
    {
        Vector3Int currentAnchorPos = GetAnchorTilePosition();

        // Only update if the anchor has moved to a new tile position
        if (currentAnchorPos != lastAnchorPos)
        {
            lastAnchorPos = currentAnchorPos;
            UpdateTilemap();
        }
    }

    // Gets the anchor's current position as a tile position
    Vector3Int GetAnchorTilePosition()
    {
        return tilemap.WorldToCell(anchorObject.transform.position);
    }

    // Updates the tilemap to generate and remove tiles around the anchor within the radius
    void UpdateTilemap()
    {
        HashSet<Vector3Int> newActiveTiles = new HashSet<Vector3Int>();

        // Iterate over a square around the anchor and check each tile position to see if it’s within radius
        Vector3Int anchorPos = GetAnchorTilePosition();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int tilePos = new Vector3Int(anchorPos.x + x, anchorPos.y + y, 0);

                // Check if the tile position is within a circular radius
                if (Vector3Int.Distance(anchorPos, tilePos) <= radius)
                {
                    newActiveTiles.Add(tilePos);

                    // Add tile if it's not already active
                    if (!activeTiles.Contains(tilePos))
                    {
                        tilemap.SetTile(tilePos, tile);
                    }
                }
            }
        }

        // Remove tiles that are no longer in the active area
        foreach (Vector3Int tilePos in activeTiles)
        {
            if (!newActiveTiles.Contains(tilePos))
            {
                tilemap.SetTile(tilePos, null); // Remove the tile
            }
        }

        // Update the active tiles set
        activeTiles = newActiveTiles;
    }
}
