using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public class ChunkData
{
    public Vector3Int chunkPosition;
    public Dictionary<Vector3Int, int> tileDepths;
    public Dictionary<Vector3Int, int> tileTemperatures;
    public int chunkHostility;
    public int chunkPopulation;
    public int chunkWeirdness;
    public GameObject chunkWaterTexture;

    public ChunkData(Vector3Int position)
    {
        chunkPosition = position;
        tileDepths = new Dictionary<Vector3Int, int>();
        tileTemperatures = new Dictionary<Vector3Int, int>();
        chunkPopulation = 0;
        chunkWeirdness = 0;
        chunkWaterTexture = null;

    }
}
public class WorldGenerator : MonoBehaviour
{
    public Tilemap seaTilemap;
    public int chunkSize = 16;
    public int radius = 50;  // Radius in chunks
    public float depthScale = 0.1f;
    public float temperatureScale = 0.1f;
    public float weirdnessScale = 0.05f;
    public List<TileObject> tileObjects; // List of TileObjects
    public int worldSeed;
    private Vector3 worldCenterPosition;
    private Vector3Int previousCenterChunkPosition;
    public Dictionary<Vector3Int, ChunkData> generatedChunks;

    public Vector3Int mouseTilePosition;


    private Vector2 waterTextureSize = new Vector2(1024, 512);
    private Vector2 tilemapSize = new Vector2(32, 16);
    public int tilemapSpritePixelsPerUnit = 32;
    public int placeWaterTextureEveryChunk = 2;
    public GameObject waterTexturePrefab;



    void Start()
    {
        generatedChunks = new Dictionary<Vector3Int, ChunkData>();

        GenerateInitialWorld();
    }

    void Update()
    {
        MaintainChunkRadius();
        DetectTileHover();
    }


    void GenerateInitialWorld()
    {
        previousCenterChunkPosition = Vector3Int.zero;

        // Generate initial chunks in a radius around the center position
        GenerateChunksInRadius(Vector3Int.zero);
    }

    public ChunkData GetChunkData(Vector3 worldPosition)
    {
        Vector3Int chunkPosition = WorldToChunkPosition(worldPosition);
        if (generatedChunks.TryGetValue(chunkPosition, out ChunkData chunkData))
        {
            return chunkData;
        }
        return null;
    }

    public Vector3Int WorldToChunkPosition(Vector3 worldPosition)
    {
        Vector3Int tilePosition = seaTilemap.WorldToCell(worldPosition);
        return new Vector3Int(
            Mathf.FloorToInt((float)tilePosition.x / chunkSize),
            Mathf.FloorToInt((float)tilePosition.y / chunkSize),
            0
        );
    }





    void GenerateChunksInRadius(Vector3Int centerChunkPosition)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector3Int chunkPosition = new Vector3Int(centerChunkPosition.x + x, centerChunkPosition.y + y, 0);


                float distance = Mathf.Sqrt(x * x + y * y);

                if (distance <= radius && !generatedChunks.ContainsKey(chunkPosition))
                {
                    GenerateChunk(chunkPosition);
                }
            }
        }
    }
    void GenerateChunk(Vector3Int chunkPosition)
    {
        // Create a new ChunkData instance for this chunk
        ChunkData chunkData = new ChunkData(chunkPosition);

        // Calculate the distance from the initial chunk position to determine hostility range
        float distanceFromInitialChunk = Vector3Int.Distance(chunkPosition, Vector3Int.zero);
        chunkData.chunkHostility = Mathf.RoundToInt(distanceFromInitialChunk);

        float seedMultiplier = worldSeed;

        // Calculate weirdness using Perlin noise
        float weirdValue = Mathf.PerlinNoise(chunkPosition.x * weirdnessScale + seedMultiplier + 2000, chunkPosition.y * weirdnessScale + seedMultiplier + 2000);
        int weird = Mathf.RoundToInt(weirdValue * 100);
        chunkData.chunkWeirdness = weird;

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                Vector3Int tilePosition = new Vector3Int(
                    chunkPosition.x * chunkSize + x,
                    chunkPosition.y * chunkSize + y,
                    0
                );

                // Calculate depth using Perlin noise
                float depthValue = Mathf.PerlinNoise(tilePosition.x * depthScale + seedMultiplier, tilePosition.y * depthScale + seedMultiplier);
                // Calculate temperature using a different Perlin noise function
                float temperatureValue = Mathf.PerlinNoise(tilePosition.x * temperatureScale + 1000 + seedMultiplier, tilePosition.y * temperatureScale + 1000 + seedMultiplier);

                // Convert Perlin noise value to integer
                int depth = Mathf.RoundToInt(depthValue * 100);
                int temperature = Mathf.RoundToInt(temperatureValue * 100);

                // Store the depth, temperature, and hostility values
                chunkData.tileDepths[tilePosition] = depth;
                chunkData.tileTemperatures[tilePosition] = temperature;

                // Determine the correct tile based on the data ranges
                TileBase selectedTile = null;
                foreach (TileObject tileObject in tileObjects)
                {
                    if (depth >= tileObject.depthRange.x && depth <= tileObject.depthRange.y &&
                        temperature >= tileObject.temperatureRange.x && temperature <= tileObject.temperatureRange.y)
                    {
                        selectedTile = tileObject.tileBase;
                        break;
                    }
                }

                // Set the tile on the tilemap
                seaTilemap.SetTile(tilePosition, selectedTile);
            }
        }

        // Add the chunk data to the generated chunks dictionary
        generatedChunks[chunkPosition] = chunkData;

        // Instantiate the structure if the weirdness is above 95
        if (chunkData.chunkWeirdness > 95)
        {
            ChangeTilesInChunk(chunkData);
            Vector3 chunkCenterWorldPosition = seaTilemap.CellToWorld(new Vector3Int(chunkPosition.x * chunkSize + chunkSize / 2, chunkPosition.y * chunkSize + chunkSize / 2, 0));
            SingletonManager.Instance.structureManager.GenerateStructure(chunkCenterWorldPosition);
        }

        // Check if both chunkPosition.x and chunkPosition.y are even
        if (chunkPosition.x % 2 == 0 && chunkPosition.y % 2 == 0)
        {
            // Calculate the world position of the center of this chunk
            Vector3 chunkCenterWorldPosition = seaTilemap.CellToWorld(new Vector3Int(
                chunkPosition.x * chunkSize + chunkSize / 2,
                chunkPosition.y * chunkSize + chunkSize / 2,
                0
            ));

            // Instantiate the small prefab at the center of the chunk
            chunkData.chunkWaterTexture = Instantiate(waterTexturePrefab, chunkCenterWorldPosition, Quaternion.identity, seaTilemap.transform);
        }
    }


    void MaintainChunkRadius()
    {
        // Get the current center chunk position
        Vector3 worldCenterPosition = transform.position;
        Vector3Int currentCenterChunkPosition = WorldToChunkPosition(worldCenterPosition);

        // Only update if the center chunk position has changed
        if (currentCenterChunkPosition != previousCenterChunkPosition)
        {
            previousCenterChunkPosition = currentCenterChunkPosition;

            // Generate new chunks in the new radius
            GenerateChunksInRadius(currentCenterChunkPosition);

            // Remove chunks that are out of the radius
            RemoveChunksOutOfRadius(currentCenterChunkPosition);
        }
    }

    void RemoveChunksOutOfRadius(Vector3Int centerChunkPosition)
    {
        List<Vector3Int> chunksToRemove = new List<Vector3Int>();

        foreach (var chunk in generatedChunks.Keys)
        {
            if (Vector3Int.Distance(chunk, centerChunkPosition) > radius)
            {
                chunksToRemove.Add(chunk);
            }
        }

        foreach (var chunk in chunksToRemove)
        {
            RemoveChunk(chunk);
        }
    }

    void RemoveChunk(Vector3Int chunkPosition)
    {
        if (generatedChunks.TryGetValue(chunkPosition, out ChunkData chunkData))
        {
            foreach (var tilePosition in chunkData.tileDepths.Keys)
            {
                // Remove the tile from the tilemap
                seaTilemap.SetTile(tilePosition, null);
            }
            if (chunkData.chunkWaterTexture != null)
            {
                Destroy(chunkData.chunkWaterTexture);
                chunkData.chunkWaterTexture = null;
            }

            // Remove the chunk data from the generated chunks dictionary
            generatedChunks.Remove(chunkPosition);
        }
    }

    void DetectTileHover()
    {
        if (Camera.main == null)
            return;

        // Get the world position of the mouse
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        // Convert the world position to a tile position
        mouseTilePosition = seaTilemap.WorldToCell(mouseWorldPosition);

        // Check if the tile exists in the generated chunks
        foreach (var chunkData in generatedChunks.Values)
        {
            if (chunkData.tileDepths.ContainsKey(mouseTilePosition) &&
                chunkData.tileTemperatures.ContainsKey(mouseTilePosition))
            {
                // Log the values to the console
                int depth = chunkData.tileDepths[mouseTilePosition];
                int temperature = chunkData.tileTemperatures[mouseTilePosition];
                int chunkPopulation = chunkData.chunkPopulation;
                float chunkWeirdness = chunkData.chunkWeirdness; // Add this line
                //Debug.Log(chunkData.chunkPosition);
                //Debug.Log($"Tile Position: {mouseTilePosition}");
                //Debug.Log($"Tile Position: {mouseTilePosition} - Depth: {depth}, Temperature: {temperature}, Hostility: {hostility}, Mob Capacity: {chunkPopulation}, Weirdness: {chunkWeirdness}, Position: {chunkData.chunkPosition}");
                break;
            }
        }
    }

    public TileBase highlightTile; // Tile to change to
    private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>();

    public void ChangeTilesInChunk(ChunkData chunkData)
    {
        foreach (var tilePosition in chunkData.tileDepths.Keys)
        {
            if (!originalTiles.ContainsKey(tilePosition))
            {
                originalTiles[tilePosition] = seaTilemap.GetTile(tilePosition);
            }
            seaTilemap.SetTile(tilePosition, highlightTile);
        }
    }

    public void RevertTilesInChunk(ChunkData chunkData)
    {
        foreach (var tilePosition in chunkData.tileDepths.Keys)
        {
            if (originalTiles.TryGetValue(tilePosition, out TileBase originalTile))
            {
                seaTilemap.SetTile(tilePosition, originalTile);
                originalTiles.Remove(tilePosition);
            }
        }
    }
}
