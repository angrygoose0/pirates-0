using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    public Tilemap seaTilemap;
    public int chunkSize = 16;
    public int radius = 50;  // Radius in chunks
    public float depthScale = 0.1f;
    public float temperatureScale = 0.1f;
    public List<TileObject> tileObjects; // List of TileObjects

    private Vector3Int initialChunkPosition;
    private Vector3Int previousCenterChunkPosition;
    private HashSet<Vector3Int> generatedChunks;
    private Dictionary<Vector3Int, int> tileDepths;
    private Dictionary<Vector3Int, int> tileTemperatures;
    private Dictionary<Vector3Int, int> tileHostility;

    void Start()
    {
        generatedChunks = new HashSet<Vector3Int>();
        tileDepths = new Dictionary<Vector3Int, int>();
        tileTemperatures = new Dictionary<Vector3Int, int>();
        tileHostility = new Dictionary<Vector3Int, int>();

        GenerateInitialWorld();
    }

    void Update()
    {
        MaintainChunkRadius();
        DetectTileHover();
    }

    void GenerateInitialWorld()
    {
        // Get the center position of the ship (ghost's position)
        Vector3 worldCenterPosition = transform.position;

        // Convert world position to chunk position
        initialChunkPosition = WorldToChunkPosition(worldCenterPosition);
        previousCenterChunkPosition = initialChunkPosition;

        // Generate initial chunks in a radius around the center position
        GenerateChunksInRadius(initialChunkPosition);
    }

    Vector3Int WorldToChunkPosition(Vector3 worldPosition)
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

                if (distance <= radius && !generatedChunks.Contains(chunkPosition))
                {
                    GenerateChunk(chunkPosition);
                    generatedChunks.Add(chunkPosition);
                }
            }
        }
    }

    void GenerateChunk(Vector3Int chunkPosition)
    {
        // Calculate the distance from the initial chunk position to determine hostility range
        float distanceFromInitialChunk = Vector3Int.Distance(chunkPosition, initialChunkPosition);
        int hostilityRange = Mathf.RoundToInt(distanceFromInitialChunk);

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
                float depthValue = Mathf.PerlinNoise(tilePosition.x * depthScale, tilePosition.y * depthScale);
                // Calculate temperature using a different Perlin noise function
                float temperatureValue = Mathf.PerlinNoise(tilePosition.x * temperatureScale + 1000, tilePosition.y * temperatureScale + 1000);

                // Convert Perlin noise value to integer
                int depth = Mathf.RoundToInt(depthValue * 100);
                int temperature = Mathf.RoundToInt(temperatureValue * 100);

                // Store the depth, temperature, and hostility values
                tileDepths[tilePosition] = depth;
                tileTemperatures[tilePosition] = temperature;
                tileHostility[tilePosition] = hostilityRange;

                // Determine the correct tile based on the data ranges
                TileBase selectedTile = null;
                foreach (TileObject tileObject in tileObjects)
                {
                    if (depth >= tileObject.depthRange.x && depth <= tileObject.depthRange.y &&
                        temperature >= tileObject.temperatureRange.x && temperature <= tileObject.temperatureRange.y &&
                        hostilityRange >= tileObject.hostilityRange.x && hostilityRange <= tileObject.hostilityRange.y)
                    {
                        selectedTile = tileObject.tileBase;
                        break;
                    }
                }

                // Set the tile on the tilemap
                seaTilemap.SetTile(tilePosition, selectedTile);
            }
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

        foreach (var chunk in generatedChunks)
        {
            if (Vector3Int.Distance(chunk, centerChunkPosition) > radius)
            {
                chunksToRemove.Add(chunk);
            }
        }

        foreach (var chunk in chunksToRemove)
        {
            RemoveChunk(chunk);
            generatedChunks.Remove(chunk);
        }
    }

    void RemoveChunk(Vector3Int chunkPosition)
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                Vector3Int tilePosition = new Vector3Int(
                    chunkPosition.x * chunkSize + x,
                    chunkPosition.y * chunkSize + y,
                    0
                );

                // Remove the tile from the tilemap and dictionaries
                seaTilemap.SetTile(tilePosition, null);
                tileDepths.Remove(tilePosition);
                tileTemperatures.Remove(tilePosition);
                tileHostility.Remove(tilePosition);
            }
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
        Vector3Int tilePosition = seaTilemap.WorldToCell(mouseWorldPosition);

        // Check if the tile exists in the dictionaries
        if (tileDepths.ContainsKey(tilePosition) && tileTemperatures.ContainsKey(tilePosition) && tileHostility.ContainsKey(tilePosition))
        {
            // Log the values to the console
            int depth = tileDepths[tilePosition];
            int temperature = tileTemperatures[tilePosition];
            int hostility = tileHostility[tilePosition];
            //Debug.Log($"Tile Position: {tilePosition} - Depth: {depth}, Temperature: {temperature}, Hostility: {hostility}");
        }
    }
}
