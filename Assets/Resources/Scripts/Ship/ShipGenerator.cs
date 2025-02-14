using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using System.Linq;



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
    public ShipData[,] raftArray = new ShipData[2, 2]
    {
        { new ShipData(), new ShipData() },   // Bottom-left is null, bottom-right is a new ShipData object
        { new ShipData(), new ShipData() },
    };

    public int xOffset = 0;
    public int yOffset = 0;

    public void AddOrUpdateShipData(Vector3Int coordinate, ShipData newShipData)
    {
        int x = coordinate.x;
        int y = coordinate.y;
        // x and y is reversed for some reason.


        int arrayX = raftArray.GetLength(0);
        int arrayY = raftArray.GetLength(1);

        if (x >= arrayX || y >= arrayY)
        {
            // Determine the new size, which is max of current and required size
            int newArrayX = Mathf.Max(x + 1, arrayX); // New row size
            int newArrayY = Mathf.Max(y + 1, arrayY);

            //Debug.Log("newArrayX" + newArrayX);
            //Debug.Log("newArrayaY" + newArrayY);



            ShipData[,] newArray = new ShipData[newArrayX, newArrayY];

            // Copy existing data from the old array to the new one
            for (int i = 0; i < arrayX; i++)
            {
                for (int j = 0; j < arrayY; j++)
                {
                    newArray[i, j] = raftArray[i, j];
                }
            }

            // Replace the old array with the new one
            raftArray = newArray;
        }

        raftArray[x, y] = newShipData;
    }




    // Method to find the position of a specific ShipData instance
    public Vector3Int FindPositionInArray(ShipData ship)
    {
        for (int x = 0; x < raftArray.GetLength(0); x++)
        {
            for (int y = 0; y < raftArray.GetLength(1); y++)
            {
                if (raftArray[x, y] == ship)
                {
                    return new Vector3Int(x, y, 0);  // Return the position as a Vector3Int (x, y, 0)
                }
            }
        }

        return Vector3Int.zero;
    }

    public GameObject grid;
    public RaftObject deafultRaft;

    public TileBase tile;
    public List<BlockObject> blockObjects; // List of all BlockObject ScriptableObjects
    public Tilemap shipTilemap;



    public GameObject raftTilePrefab;
    public GameObject healthBarPrefab;
    public GameObject canvas;


    public TileBase transparentTile;
    private List<Vector3Int> placedHoverTiles = new List<Vector3Int>(); // List to store tile positions
    private Vector3Int? lastPlacedCenterTile = null;
    public Tilemap temporaryTilemap;
    private Vector3Int centerTile;
    public Vector3Int? newRaftPosition;
    private bool hasTile;
    private Vector3 mouseWorldPos;
    private Vector3Int tilePos;
    // Public read-only property
    public Vector3Int MouseTilePos
    {
        get { return tilePos; }
    }





    public float[,] ship = new float[,]
    {
    };
    public int raftTileSize = 5;
    private float[,] singularRaftArray;


    private Dictionary<Vector3Int, GameObject> tileToBlockPrefabMap;
    public List<GameObject> mastBlocks; // List to store blocks of type Mast
    public List<GameObject> cannonBlocks;



    void Start()
    {
        CombineRaftTilesIntoShip();

        tileToBlockPrefabMap = new Dictionary<Vector3Int, GameObject>();
        mastBlocks = new List<GameObject>();
        cannonBlocks = new List<GameObject>();
        GenerateTilemap(ship);
        CenterGhostOnShip();
        SortBlocks(); // Find all mast blocks after generating the ship



    }

    void Update()
    {

        // Get the mouse position in world space
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z); // Adjust Z based on your camera's position

        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // Convert world position to shiptilemap cell position in isometric grid
        tilePos = shipTilemap.WorldToCell(mouseWorldPos);

        UpdateRaftTimers();

        if (!SingletonManager.Instance.gameStart.gameStarted)
        {
            HandleHoverEffect();

            if (Input.GetMouseButtonDown(0))
            {

                if (lastPlacedCenterTile == centerTile && newRaftPosition != null)
                {
                    lastPlacedCenterTile = null;
                    ShipData newShipData = new ShipData();

                    AddOrUpdateShipData(newRaftPosition.Value, newShipData);

                    CombineRaftTilesIntoShip();
                    GenerateTilemap(ship);
                    CenterGhostOnShip();
                    ZoomToFitTilemap();
                }

                else if (hasTile)
                {
                    float value = ship[-tilePos.y, tilePos.x];

                    value = (value % 4) + 1;
                    ship[-tilePos.y, tilePos.x] = value;


                    GenerateTilemap(ship);

                }
            }
        }
    }

    private void HandleHoverEffect()
    {


        centerTile = GetNearestGridCenter(tilePos);


        hasTile = shipTilemap.HasTile(centerTile);
        newRaftPosition = IsTileAdjacentToShip(centerTile);



        // If a valid location to place the 5x5 grid is found
        if (!hasTile && newRaftPosition != null)
        {
            if (lastPlacedCenterTile != centerTile)
            {
                RemovePlacedTiles();
                Place5x5Grid(centerTile);
                lastPlacedCenterTile = centerTile;
            }

        }
        else if (lastPlacedCenterTile != null)
        {
            RemovePlacedTiles();
            lastPlacedCenterTile = null;
        }

    }

    public CinemachineVirtualCamera editModeCamera;
    public float tileSize = 1f;

    private void ZoomToFitTilemap()
    {
        int rows = ship.GetLength(1);
        int cols = ship.GetLength(0);
        // Calculate the width and height of the tilemap in world units
        float tilemapWidth = cols * tileSize;
        float tilemapHeight = rows * tileSize;

        // Camera's aspect ratio (width / height of the screen)
        float screenAspect = Screen.width / (float)Screen.height;

        // Calculate the orthographic size based on the larger dimension (width or height)
        float orthoSizeByHeight = tilemapHeight / 2f;
        float orthoSizeByWidth = (tilemapWidth / screenAspect) / 2f;

        // Set the orthographic size to the larger value to ensure the whole tilemap fits
        editModeCamera.m_Lens.OrthographicSize = Mathf.Max(orthoSizeByHeight, orthoSizeByWidth);
    }


    public void Place5x5Grid(Vector3Int center)
    {

        int halfSize = 2;  // 5x5 grid has a radius of 2 tiles from the center

        for (int x = -halfSize; x <= halfSize; x++)  // Loop from -2 to 2 in the X direction
        {
            for (int y = -halfSize; y <= halfSize; y++)  // Loop from -2 to 2 in the Y direction
            {
                Vector3Int tilePosition = new Vector3Int(center.x + x, center.y + y, center.z);
                temporaryTilemap.SetTile(tilePosition, transparentTile);  // Place a tile at the calculated position
                placedHoverTiles.Add(tilePosition);  // Store the tile position
            }
        }
    }


    public void RemovePlacedTiles()
    {
        foreach (Vector3Int tilePosition in placedHoverTiles)
        {
            temporaryTilemap.SetTile(tilePosition, null);  // Remove the tile at the stored position
        }
        placedHoverTiles.Clear();  // Clear the list after removal

    }


    public Vector3Int GetNearestGridCenter(Vector3Int input)
    {
        Vector3Int baseCoordinate = new Vector3Int(2, -2, 0);
        // Calculate the relative position from the base coordinate
        Vector3 relativePosition = input - baseCoordinate;

        // Round the relative position to the nearest multiple of raftTileSize
        int nearestX = Mathf.RoundToInt(relativePosition.x / raftTileSize) * raftTileSize;
        int nearestY = Mathf.RoundToInt(relativePosition.y / raftTileSize) * raftTileSize;

        // Add the baseCoordinate back to get the world position of the nearest grid center
        return new Vector3Int(nearestX + baseCoordinate.x, nearestY + baseCoordinate.y, baseCoordinate.z);
    }


    private Vector3Int? IsTileAdjacentToShip(Vector3Int tilePos)
    {
        // Define the four directions (up, down, left, right)
        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(0, 3, 0),   // Up
        new Vector3Int(0, -3, 0),  // Down
        new Vector3Int(-3, 0, 0),  // Left
        new Vector3Int(3, 0, 0)    // Right
        };

        // Loop through each direction
        foreach (Vector3Int direction in directions)
        {

            Vector3Int checkPos = tilePos + direction;  // Calculate position in the given direction

            // Check if there is a tile at the checkPos in the tilemap
            if (shipTilemap.HasTile(checkPos))
            {
                ShipData closestShipData = GetShipDataAtTile(shipTilemap.CellToWorld(checkPos));
                Vector3Int arrayPosition = FindPositionInArray(closestShipData);



                Vector3Int newDirection = direction / -3;
                newDirection = new Vector3Int(-newDirection.y, newDirection.x, 0);

                Vector3Int newArrayPosition = arrayPosition + newDirection;
                newArrayPosition.x -= xOffset;
                newArrayPosition.y -= yOffset;


                if (Input.GetMouseButtonDown(0))
                {
                    //Debug.Log("array" + arrayPosition);
                    // Debug.Log("newArray" + newArrayPosition);
                }

                return newArrayPosition;  // A tile was found in this direction within 5 tiles
            }

        }

        // No tile found within 5 tiles in any direction
        return null;
    }

    private ShipData GetShipDataAtTile(Vector3 worldPos)
    {
        // Cast a ray at theposition to detect colliders
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // Check if the collider hit belongs to a raft tile
            foreach (var entry in raftTileDict)
            {
                if (hit.collider == entry.Key.GetComponent<Collider2D>())
                {
                    return entry.Value;  // Return the ShipData associated with this raft tile
                }
            }
        }
        return null;
    }


    public GameObject GenerateIndividualRaft(int size, Vector3Int position)
    {
        // Instantiate the raft at the given position
        GameObject raftTileInstance = Instantiate(raftTilePrefab, shipTilemap.transform);

        // Move the entire raft tile to the correct position in the world
        raftTileInstance.transform.position = shipTilemap.CellToWorld(position);

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


    private void DestroyRaftTile(GameObject raftTileObject)
    {
        if (raftTileDict.ContainsKey(raftTileObject))
        {
            ShipData shipData = raftTileDict[raftTileObject];
            Destroy(shipData.healthBar.gameObject);


            raftTileDict.Remove(raftTileObject);
            Destroy(raftTileObject);

            bool found = false;
            // Loop through the raftArray to find and remove the ShipData
            for (int x = 0; x < raftArray.GetLength(0); x++)
            {
                for (int y = 0; y < raftArray.GetLength(1); y++)
                {
                    if (raftArray[x, y] == shipData)
                    {
                        // Set the element in the array to null once found
                        raftArray[x, y] = null;
                        found = true;  // Set the flag to true
                        break;  // Break out of the inner loop
                    }
                }
                if (found) break;  // Break out of the outer loop if found
            }

            CombineRaftTilesIntoShip();
            GenerateTilemap(ship);
            CenterGhostOnShip();
            SortBlocks();

        }
    }

    public float GenerateFloatWithSeed(int x, int y, int seed)
    {
        // Combine x, y, and seed into a single unique seed value
        int combinedSeed = x * 73856093 ^ y * 19349663 ^ seed;

        // Initialize the random number generator with the combined seed
        Random.InitState(combinedSeed);

        // Return a random float between 0 and 1
        return Random.value; // or you can use Random.Range(min, max) to get a specific range
    }
    private void CombineRaftTilesIntoShip()
    {

        foreach (KeyValuePair<GameObject, ShipData> entry in raftTileDict)
        {
            GameObject raftTile = entry.Key;

            // Destroy the GameObject
            Destroy(raftTile);

            // Optionally, clean up ShipData if necessary
            // Destroy(shipData); // Uncomment if ShipData needs to be explicitly destroyed
        }

        // Clear the dictionary after destroying the GameObjects
        raftTileDict.Clear();

        raftTileSize = 5; // Size of individual raft tiles (5x5)

        int shipSizeX = raftArray.GetLength(0) * raftTileSize; // 5 is the size of each individual raft tile.
        int shipSizeY = raftArray.GetLength(1) * raftTileSize;
        ship = new float[shipSizeX, shipSizeY];


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

                    shipData.tilemapRenderer.sortingOrder = -(int)(newRaftTile.transform.position.y * 1000);




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

                    for (int x = 0; x < raftTileSize; x++)
                    {
                        for (int y = 0; y < raftTileSize; y++)
                        {
                            int shipX = i * raftTileSize + x;
                            int shipY = j * raftTileSize + y;
                            int randomInt = Mathf.RoundToInt(100f * GenerateFloatWithSeed(shipX, shipY, 42));

                            if (randomInt <= 92)
                            {
                                ship[shipX, shipY] = 1f; //empty
                            }
                            else if (randomInt <= 98)
                            {
                                ship[shipX, shipY] = 3f; // payload
                            }
                            else if (randomInt <= 100)
                            {
                                ship[shipX, shipY] = 2f; // cannon
                            }


                        }
                    }
                }
            }
        }
    }

    public void GenerateTilemap(float[,] ship)
    {
        SingletonManager.Instance.blockManager.DestroyAllBlocks();

        shipTilemap.ClearAllTiles();
        tileToBlockPrefabMap.Clear();
        mastBlocks.Clear(); // Clear the list in case of regeneration
        cannonBlocks.Clear();

        int rows = ship.GetLength(0);
        int cols = ship.GetLength(1);

        /*
        PolygonCollider2D shipCollider = shipTilemap.GetComponent<PolygonCollider2D>();

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
                    shipTilemap.SetTile(tilePosition, tile);

                    if (ship[y, x] > 1.0f)
                    {
                        // Convert tilemap position to world position
                        Vector3 worldPosition = shipTilemap.CellToWorld(tilePosition) + shipTilemap.tileAnchor;
                        worldPosition.z = 0; // Ensure the block is at the correct Z position if needed

                        // Adjust the position as required
                        worldPosition.x -= 0.5f;
                        worldPosition.y -= 0.225f;

                        // Find the correct BlockObject based on the ID
                        BlockObject blockObject = blockObjects.Find(b => Mathf.Approximately(b.id, ship[y, x]));

                        if (blockObject != null)
                        {
                            GameObject blockInstance = SingletonManager.Instance.blockManager.CreateNewBlock(blockObject, worldPosition, shipTilemap.transform);

                            // Store the mapping between the tile position and the block instance
                            tileToBlockPrefabMap[tilePosition] = blockInstance;
                        }
                        else
                        {
                            Debug.LogWarning($"BlockObject with ID {ship[y, x]} not found.");
                        }




                    }
                }
            }
        }
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
        //shipData.healthBar.ModifyHealth(roundedResult);
    }

    public void ApplyImpact(GameObject raftTile, float damageMagnitude)
    {
        ShipData shipData = raftTileDict[raftTile];
        SingletonManager.Instance.feedbackManager.ShipDamagedFeedback(damageMagnitude);
        AbilityData fragility = SingletonManager.Instance.abilityManager.GetAbilityData(Ability.Fragility);
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
                if (shipData.hp <= 0)
                {
                    DestroyRaftTile(raftTile);
                }
                UpdateShipHealth(shipData);
            }
        }
        else
        {
            shipData.currentDamage = damageMagnitude;
            StartCoroutine(DamageCoroutine(raftTile));
        }
    }



    private IEnumerator DamageCoroutine(GameObject raftTile)
    {
        ShipData shipData = raftTileDict[raftTile];

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
            DestroyRaftTile(raftTile);
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

    public GameObject trailPrefab;

    public void MakeTrailEffects(GameObject blockGameObject)
    {
        if (SingletonManager.Instance.blockManager.blockDictionary.TryGetValue(blockGameObject, out BlockData blockData))
        {
            Color effectColor = SingletonManager.Instance.itemManager.itemDictionary[blockData.itemsInBlock[0]].itemObject.effectColor;

            foreach (GameObject cannonBlock in cannonBlocks)
            {
                GameObject trailObject = Instantiate(trailPrefab, blockGameObject.transform.position, Quaternion.identity, shipTilemap.transform);
                blockData.trailObjectList.Add(trailObject);
                StartCoroutine(CreateTrail(trailObject, cannonBlock.transform, effectColor));
            }
        }
    }

    private IEnumerator CreateTrail(GameObject trailObject, Transform endBlock, Color effectColor) // get color
    {
        Vector3 startPosition = trailObject.transform.position;

        LineRenderer lineRenderer = trailObject.GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition);

        lineRenderer.material.SetColor("_GlowColor", effectColor);
        //lineRenderer.material.SetFloat("_GlowIntensity", 3f);



        float progress = 0.9f;
        float duration = Vector3.Distance(startPosition, endBlock.position);  // Longer distance takes more time
        float accelerationFactor = 50.0f;  // Adjust this factor for more/less acceleration

        while (progress < 1f)
        {
            // Increase progress over time, non-linearly, to simulate acceleration
            progress += Time.deltaTime / duration;
            float acceleratedProgress = Mathf.Pow(progress, accelerationFactor);  // Non-linear curve for acceleration

            Vector3 currentPoint = Vector3.Lerp(startPosition, endBlock.position, acceleratedProgress);
            lineRenderer.SetPosition(1, currentPoint);
            yield return null;
        }

        // Ensure the line ends exactly at the cannon block
        lineRenderer.SetPosition(1, endBlock.position);

        float multiplier = 1f;
        Vector3 newEndPosition = endBlock.position + new Vector3(0.0f, 0.125f, 0.0f);
        SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(endBlock.position, multiplier);
    }




    public void CenterGhostOnShip()
    {
        // Calculate the center of the ship
        int rows = ship.GetLength(0);
        int cols = ship.GetLength(1);
        Vector3Int centerTilePosition = new Vector3Int(cols / 2, -rows / 2, 0);

        // Convert center tile position to world position
        Vector3 worldCenterPosition = shipTilemap.CellToWorld(centerTilePosition) + shipTilemap.tileAnchor;
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
        tileToBlockPrefabMap.TryGetValue(tilePosition, out GameObject blockGameObject);
        return blockGameObject;
    }

    public void UpdateBlockEffects()
    {
        SingletonManager.Instance.abilityManager.abilityList.Clear();
        foreach (var kvp in tileToBlockPrefabMap)
        {
            GameObject blockGameObject = kvp.Value;

            BlockData blockData = SingletonManager.Instance.blockManager.blockDictionary[blockGameObject];
            if (blockData.itemsInBlock == null || blockData.itemsInBlock.Count != 1 || blockData.blockObject.blockType != BlockType.Payload)
            {
                continue;
            }

            ItemData blockItemData = SingletonManager.Instance.itemManager.itemDictionary[blockData.itemsInBlock[0]];

            if (blockItemData.itemObject.abilityList.Count < 1)
            {
                continue;
            }
            foreach (AbilityData ability in blockItemData.itemObject.abilityList)
            {
                SingletonManager.Instance.abilityManager.AddOrUpdateAbility(ability.ability, ability.value);
            }
        }

    }


    public List<(Vector3Int position, Direction direction)> GetInteractableNeighbors(Vector3Int currentTilePosition)
    {
        List<(Vector3Int position, Direction direction)> interactableNeighbors = new List<(Vector3Int position, Direction direction)>();


        foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
        {
            Vector2 vector = direction.ToVector2();
            Vector3Int neighborPosition = currentTilePosition + new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), 0);

            if (IsTileInteractable(neighborPosition))
            {
                interactableNeighbors.Add((neighborPosition, direction));
            }
        }

        return interactableNeighbors;
    }

    private void SortBlocks()
    {
        mastBlocks.Clear();
        cannonBlocks.Clear();
        foreach (var entry in tileToBlockPrefabMap)
        {
            GameObject blockInstance = entry.Value;
            BlockData blockData = SingletonManager.Instance.blockManager.blockDictionary[blockInstance];

            if (blockData.blockObject.blockType == BlockType.Mast)
            {
                mastBlocks.Add(blockInstance);
            }
            else if (blockData.blockObject.blockType == BlockType.Cannon)
            {
                cannonBlocks.Add(blockInstance);
            }


        }
    }

    // Method to get the list of mast blocks for testing or other purposes
    public List<GameObject> GetMastBlocks()
    {
        return mastBlocks;
    }
    public List<GameObject> GetCannonBlocks()
    {
        return cannonBlocks;
    }
}
