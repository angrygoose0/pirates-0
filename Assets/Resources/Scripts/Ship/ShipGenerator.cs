using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ShipGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase tile;
    public Vector3Int offset;
    public GameObject blockPrefab; // Reference to the block prefab
    public List<BlockObject> blockObjects; // List of all BlockObject ScriptableObjects

    public float[,] ship = new float[,]
    {
        { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        { 1.0f, 1.0f, 3.0f, 1.0f, 1.0f },
        { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        { 0.0f, 1.0f, 1.0f, 2.0f, 2.1f }
    };

    private Dictionary<Vector3Int, GameObject> tileToBlockPrefabMap;
    public List<GameObject> mastBlocks; // List to store blocks of type Mast

    public enum Direction
    {
        S, SE, E, NE, N, NW, W, SW
    }

    void Start()
    {
        tileToBlockPrefabMap = new Dictionary<Vector3Int, GameObject>();
        mastBlocks = new List<GameObject>();
        GenerateTilemap(ship);
        CenterGhostOnShip();
        FindMastBlocks(); // Find all mast blocks after generating the ship
    }

    public void GenerateTilemap(float[,] ship)
    {
        tilemap.ClearAllTiles();
        tileToBlockPrefabMap.Clear();
        mastBlocks.Clear(); // Clear the list in case of regeneration

        int rows = ship.GetLength(0);
        int cols = ship.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3Int tilePosition = new Vector3Int(x, -y, 0) + offset;

                if (ship[y, x] > 0)
                {
                    tilemap.SetTile(tilePosition, tile);

                    if (ship[y, x] > 1.0f)
                    {
                        // Convert tilemap position to world position
                        Vector3 worldPosition = tilemap.CellToWorld(tilePosition) + tilemap.tileAnchor;
                        worldPosition.z = 1; // Ensure the block is at the correct Z position if needed

                        // Adjust the position as required
                        worldPosition.x -= 0.5f;
                        worldPosition.y -= 0.225f;

                        // Instantiate the block prefab
                        GameObject blockInstance = Instantiate(blockPrefab, worldPosition, Quaternion.identity);

                        // Find the correct BlockObject based on the ID
                        BlockObject blockObject = blockObjects.Find(b => Mathf.Approximately(b.id, ship[y, x]));

                        if (blockObject != null)
                        {
                            blockPrefabScript blockScript = blockInstance.GetComponent<blockPrefabScript>();
                            blockScript.blockObject = blockObject;
                        }
                        else
                        {
                            Debug.LogWarning($"BlockObject with ID {ship[y, x]} not found.");
                        }

                        // Store the mapping between the tile position and the block instance
                        tileToBlockPrefabMap[tilePosition] = blockInstance;
                    }
                }
            }
        }
    }

    public void CenterGhostOnShip()
    {
        // Calculate the center of the ship
        int rows = ship.GetLength(0);
        int cols = ship.GetLength(1);
        Vector3Int centerTilePosition = new Vector3Int(cols / 2, -rows / 2, 0) + offset;

        // Convert center tile position to world position
        Vector3 worldCenterPosition = tilemap.CellToWorld(centerTilePosition) + tilemap.tileAnchor;
        worldCenterPosition.z = 0; // Ensure the center position is at the correct Z position if needed

        // Adjust the position as required (based on tilemap anchor)
        worldCenterPosition.x -= 0.5f;
        worldCenterPosition.y -= 0.225f;

        // Set the position of the "ghost" GameObject
        transform.position = worldCenterPosition;
    }

    public bool IsTileWalkable(Vector3Int tilePosition)
    {
        Vector3Int arrayPosition = tilePosition - offset;
        if (arrayPosition.x >= 0 && arrayPosition.x < ship.GetLength(1) && -arrayPosition.y >= 0 && -arrayPosition.y < ship.GetLength(0))
        {
            return Mathf.Approximately(ship[-arrayPosition.y, arrayPosition.x], 1.0f);
        }
        return false;
    }

    public bool IsTileInteractable(Vector3Int tilePosition)
    {
        Vector3Int arrayPosition = tilePosition - offset;
        if (arrayPosition.x >= 0 && arrayPosition.x < ship.GetLength(1) && -arrayPosition.y >= 0 && -arrayPosition.y < ship.GetLength(0))
        {
            return ship[-arrayPosition.y, arrayPosition.x] > 1.0f;
        }
        return false;
    }

    public GameObject GetBlockPrefabAtTile(Vector3Int tilePosition)
    {
        tileToBlockPrefabMap.TryGetValue(tilePosition, out GameObject blockPrefab);
        return blockPrefab;
    }

    public List<(Vector3Int position, Direction direction)> GetInteractableNeighbors(Vector3Int currentTilePosition)
    {
        List<(Vector3Int position, Direction direction)> interactableNeighbors = new List<(Vector3Int position, Direction direction)>();
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int( 0, -1, 0), // S
            new Vector3Int( 1, -1, 0), // SE
            new Vector3Int( 1,  0, 0), // E
            new Vector3Int( 1,  1, 0), // NE
            new Vector3Int( 0,  1, 0), // N
            new Vector3Int(-1,  1, 0), // NW
            new Vector3Int(-1,  0, 0), // W
            new Vector3Int(-1, -1, 0)  // SW
        };

        Direction[] directionNames = new Direction[]
        {
            Direction.S, Direction.SE, Direction.E, Direction.NE, Direction.N, Direction.NW, Direction.W, Direction.SW
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3Int neighborPosition = currentTilePosition + directions[i];
            if (IsTileInteractable(neighborPosition))
            {
                interactableNeighbors.Add((neighborPosition, directionNames[i]));
            }
        }

        return interactableNeighbors;
    }

    private void FindMastBlocks()
    {
        mastBlocks.Clear();
        foreach (var entry in tileToBlockPrefabMap)
        {
            GameObject blockInstance = entry.Value;
            blockPrefabScript blockScript = blockInstance.GetComponent<blockPrefabScript>();
            if (blockScript != null && blockScript.blockObject != null && blockScript.blockObject.blockType == BlockType.Mast)
            {
                mastBlocks.Add(blockInstance);
            }
        }
    }

    // Method to get the list of mast blocks for testing or other purposes
    public List<GameObject> GetMastBlocks()
    {
        return mastBlocks;
    }
}
