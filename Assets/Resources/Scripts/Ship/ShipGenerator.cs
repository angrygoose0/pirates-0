using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;




[System.Serializable]
public class ShipData
{
    public RaftObject raftObject;
    public float hp = 0f;

    public bool isDamaged = false;
    public float currentDamage = 0f;
    public bool invincible = false;
    public float timeSinceLastDamage = 0f; // Time since the last damage was taken
    public TilemapRenderer tilemapRenderer = null;
    public HealthBar healthBar = null;


}

public class ShipGenerator : MonoBehaviour
{
    public Dictionary<GameObject, ShipData> raftTileDict = new Dictionary<GameObject, ShipData>();
    public ShipData[,] raftArray = new ShipData[3, 2]
    {
        { new ShipData(), new ShipData() },   // Bottom-left is null, bottom-right is a new ShipData object
        { new ShipData(), new ShipData() },
        { new ShipData(), null }
    };

    public FeedbackManager feedbackManager;

    public GameObject shipTilemapObject;
    public GameObject grid;
    public RaftObject deafultRaft;

    public TileBase tile;
    public GameObject blockPrefab; // Reference to the block prefab
    public List<BlockObject> blockObjects; // List of all BlockObject ScriptableObjects
    public Tilemap tilemap;
    public AbilityManager abilityManager;

    public GameObject raftTilePrefab;
    public GameObject healthBarPrefab;
    public GameObject canvas;

    public float[,] ship = new float[,]
    {
        { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,},
        { 4.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,},
        { 1.0f, 1.0f, 3.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, },
        { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, },
        { 1.0f, 1.0f, 1.0f, 2.0f, 4f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, },
        { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,},
        { 4.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,},
        { 1.0f, 1.0f, 3.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,},
        { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,},
        { 1.0f, 1.0f, 1.0f, 2.0f, 4.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,}

    };
    public int raftTileSize = 5;
    private float[,] singularRaftArray;


    private Dictionary<Vector3Int, GameObject> tileToBlockPrefabMap;
    public List<GameObject> mastBlocks; // List to store blocks of type Mast

    public enum Direction
    {
        S, SE, E, NE, N, NW, W, SW
    }

    void Start()
    {
        //new
        int shipSizeX = raftArray.GetLength(0) * raftTileSize; // 5 is the size of each individual raft tile.
        int shipSizeY = raftArray.GetLength(1) * raftTileSize;
        ship = new float[shipSizeX, shipSizeY];

        tilemap = shipTilemapObject.GetComponent<Tilemap>();
        CombineRaftTilesIntoShip();




        tileToBlockPrefabMap = new Dictionary<Vector3Int, GameObject>();
        mastBlocks = new List<GameObject>();
        GenerateTilemap(ship);
        CenterGhostOnShip();
        FindMastBlocks(); // Find all mast blocks after generating the ship



    }

    void Update()
    {
        UpdateRaftTimers();
    }

    public void UpdateRaftTimers()
    {
        // Loop through each key-value pair in the dictionary
        foreach (KeyValuePair<GameObject, ShipData> entry in raftTileDict)
        {
            GameObject raftTile = entry.Key;        // The key (GameObject)
            ShipData shipData = entry.Value;    // The value (ShipData)

            // Increment the timer
            if (!shipData.isDamaged)
            {
                shipData.timeSinceLastDamage += Time.deltaTime;
            }
        }
    }

    private void UpdateShipHealth(ShipData shipData)
    {

        float result = shipData.hp / shipData.raftObject.health;

        float roundedResult = Mathf.Round(result * 1000f) / 1000f;
        shipData.healthBar.ModifyHealth(roundedResult);
    }


    public GameObject GenerateIndividualRaft(int size, Vector3Int position)
    {
        // Instantiate the raft at the given position
        GameObject raftTileInstance = Instantiate(raftTilePrefab, grid.transform);

        // Move the entire raft tile to the correct position in the world
        raftTileInstance.transform.position = tilemap.CellToWorld(position);

        Tilemap raftTilemap = raftTileInstance.GetComponent<Tilemap>();

        // Iterate through the grid size and set the tile at each position
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Convert to isometric coordinates (Z as Y)
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                // Set the tile at the calculated position on the tilemap
                raftTilemap.SetTile(tilePosition, tile);
            }
        }

        return raftTileInstance;
    }


    private void CombineRaftTilesIntoShip()
    {
        int raftTileSize = 5; // Size of individual raft tiles (5x5)

        // Initialize the ship array to default value 0
        for (int x = 0; x < ship.GetLength(0); x++)
        {
            for (int y = 0; y < ship.GetLength(1); y++)
            {
                ship[x, y] = 0f; // Default value
            }
        }

        // Iterate over the raftArray and place individual rafts
        for (int i = 0; i < raftArray.GetLength(0); i++)
        {
            for (int j = 0; j < raftArray.GetLength(1); j++)
            {
                ShipData shipData = raftArray[i, j];

                if (shipData != null)
                {
                    // Fix the 90-degree counterclockwise rotation by swapping i and j
                    Vector3Int raftPosition = new Vector3Int(j * raftTileSize, -i * raftTileSize, 0);

                    // Generate the individual raft at the correct position
                    GameObject newRaftTile = GenerateIndividualRaft(raftTileSize, raftPosition);


                    GameObject newHealthBar = Instantiate(healthBarPrefab, newRaftTile.transform.position, Quaternion.identity, canvas.transform);
                    HealthBar healthBarScript = newHealthBar.GetComponent<HealthBar>();

                    newRaftTile.transform.position += new Vector3(2, -1, 0);

                    shipData.tilemapRenderer = newRaftTile.GetComponent<TilemapRenderer>();
                    shipData.raftObject = deafultRaft;
                    shipData.hp = shipData.raftObject.health;
                    shipData.healthBar = healthBarScript;
                    StartCoroutine(HealthRegenCoroutine(shipData));

                    raftTileDict.Add(newRaftTile, shipData);



                    PolygonCollider2D shipCollider = newRaftTile.GetComponent<PolygonCollider2D>();

                    Vector2[] colliderPoints = new Vector2[]
                    {
                        new Vector2(raftTileSize/2f-0.5f, raftTileSize/4f),
                        new Vector2(-0.5f, 0f),
                        new Vector2(raftTileSize/2f-0.5f, raftTileSize/-4f),
                        new Vector2((raftTileSize/2f-0.5f + raftTileSize/2f-0.5f + 0.5f), 0f),
                    };

                    // add on offset of -2,1 to colliders to make it work, idk why?
                    for (int k = 0; k < colliderPoints.Length; k++)
                    {
                        colliderPoints[k] += new Vector2(-2, 1);
                    }


                    shipCollider.points = colliderPoints;


                    // Populate the ship array for the combined tilemap
                    for (int x = 0; x < raftTileSize; x++)
                    {
                        for (int y = 0; y < raftTileSize; y++)
                        {
                            // Calculate the correct position in the massive ship array
                            int shipX = i * raftTileSize + x;
                            int shipY = j * raftTileSize + y;

                            // Avoid out-of-bounds issues by checking if the coordinates fit in the ship array
                            if (shipX < ship.GetLength(0) && shipY < ship.GetLength(1))
                            {
                                // Copy the tile value if valid, otherwise set to 0
                                ship[shipX, shipY] = 1.0f;
                            }
                        }
                    }
                }
            }
        }
    }

    public void ApplyImpact(GameObject raftTile, float damageMagnitude)
    {
        ShipData shipData = raftTileDict[raftTile];
        feedbackManager.ShipDamagedFeedback(damageMagnitude);
        AbilityData fragility = abilityManager.GetAbilityData(Ability.Fragility);
        if (fragility != null)
        {
            damageMagnitude = damageMagnitude * fragility.value;
        }

        if (shipData.isDamaged)
        {
            if (damageMagnitude > shipData.currentDamage)
            {
                float damage = damageMagnitude - shipData.currentDamage;
                shipData.hp -= damage;
                UpdateShipHealth(shipData);
            }
        }
        else
        {
            shipData.currentDamage = damageMagnitude;
            StartCoroutine(DamageCoroutine(shipData));
        }
    }

    private IEnumerator DamageCoroutine(ShipData shipData)
    {
        shipData.isDamaged = true;
        shipData.timeSinceLastDamage = 0f; // Reset the timer

        // Apply damage
        shipData.hp -= shipData.currentDamage;
        UpdateShipHealth(shipData);

        shipData.tilemapRenderer.material.SetFloat("_WhiteAmount", 1f);

        // Check if health is less than or equal to zero
        if (shipData.hp <= 0)
        {
            Debug.Log("destroy this tile");
            yield break;  // Stop coroutine execution if the ship is destroyed
        }

        // Wait for invincibility duration
        yield return new WaitForSeconds(0.1f);

        shipData.tilemapRenderer.material.SetFloat("_WhiteAmount", 0f);

        // Reset damage
        shipData.currentDamage = 0f;
        shipData.isDamaged = false;
    }

    private IEnumerator HealthRegenCoroutine(ShipData shipData)
    {
        while (true)
        {
            // Check if the ship can regenerate health
            if (shipData.hp < shipData.raftObject.health && shipData.timeSinceLastDamage >= shipData.raftObject.regenDelay)
            {
                // Regenerate health over time
                shipData.hp += shipData.raftObject.healthRegenRate * Time.deltaTime;
                shipData.hp = Mathf.Min(shipData.hp, shipData.raftObject.health);
            }

            yield return null; // Wait for the next frame
        }
    }

    public void GenerateTilemap(float[,] ship)
    {

        tilemap.ClearAllTiles();
        tileToBlockPrefabMap.Clear();
        mastBlocks.Clear(); // Clear the list in case of regeneration

        int rows = ship.GetLength(0);
        int cols = ship.GetLength(1);

        /*
        PolygonCollider2D shipCollider = shipTilemapObject.GetComponent<PolygonCollider2D>();

        Vector2[] colliderPoints = new Vector2[]
        {
            new Vector2(cols/2f-0.5f, cols/4f),
            new Vector2(-0.5f, 0f),
            new Vector2(rows/2f-0.5f, rows/-4f),
            new Vector2((cols/2f-0.5f + rows/2f-0.5f + 0.5f), (cols-rows)/4f),
            //new Vector2(cols/2f-0.5f, cols/4f),
        };

        shipCollider.points = colliderPoints;
        */

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3Int tilePosition = new Vector3Int(x, -y, 0);

                if (ship[y, x] > 0)
                {
                    tilemap.SetTile(tilePosition, tile);

                    if (ship[y, x] > 1.0f)
                    {
                        // Convert tilemap position to world position
                        Vector3 worldPosition = tilemap.CellToWorld(tilePosition) + tilemap.tileAnchor;
                        worldPosition.z = 0; // Ensure the block is at the correct Z position if needed

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
        Vector3Int centerTilePosition = new Vector3Int(cols / 2, -rows / 2, 0);

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
        Vector3Int arrayPosition = tilePosition;
        if (arrayPosition.x >= 0 && arrayPosition.x < ship.GetLength(1) && -arrayPosition.y >= 0 && -arrayPosition.y < ship.GetLength(0))
        {
            return Mathf.Approximately(ship[-arrayPosition.y, arrayPosition.x], 1.0f);
        }
        return false;
    }

    public bool IsTileInteractable(Vector3Int tilePosition)
    {
        Vector3Int arrayPosition = tilePosition;
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

    public void UpdateBlockEffects()
    {
        abilityManager.abilityList.Clear();
        foreach (var kvp in tileToBlockPrefabMap)
        {
            GameObject blockPrefab = kvp.Value;

            blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
            if (blockScript.itemPrefabObject == null || blockScript.itemPrefabObject.Count != 1 || blockScript.blockObject.blockType != BlockType.Payload)
            {
                continue;
            }

            GameObject blockItem = blockScript.itemPrefabObject[0];
            ItemScript blockItemScript = blockItem.GetComponent<ItemScript>();
            ItemObject blockItemObject = blockItemScript.itemObject;

            if (blockItemObject.abilityList.Count < 1)
            {
                continue;
            }
            foreach (AbilityData ability in blockItemObject.abilityList)
            {
                abilityManager.AddOrUpdateAbility(ability.ability, ability.value);
            }
        }

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
