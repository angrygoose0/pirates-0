using UnityEngine;
using System.Collections.Generic;

public class CreatureManager : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public GameObject trackedObject;
    public List<GameObject> creatures; // List of all creatures
    public List<CreatureObject> creatureObjects;
    public int minRadius = 5;
    public int maxRadius = 10;
    private ChunkData currentChunk;
    private HashSet<Vector3Int> viableChunks = new HashSet<Vector3Int>();
    private Dictionary<GameObject, ChunkData> creatureChunks = new Dictionary<GameObject, ChunkData>();
    public int globalMobCount; // New global mob count
    public int maxGlobalMobCount = 70; // Maximum global mob count
    public int maxGlobalChunkPopulation = 50;
    public GameObject creaturePrefab;

    void Start()
    {
        globalMobCount = creatures.Count; // Initialize the global mob count
    }

    void Update()
    {
        TrackObjectChunk();
        UpdateCreatureChunks();

        if (globalMobCount < maxGlobalMobCount)
        {
            mobSpawner();
        }

        HandleDespawning();
    }

    void TrackObjectChunk()
    {
        Vector3 objectPosition = trackedObject.transform.position;
        ChunkData newChunk = worldGenerator.GetChunkData(objectPosition);

        if (newChunk != null && newChunk != currentChunk)
        {
            if (currentChunk != null)
            {

                //worldGenerator.RevertTilesInChunk(currentChunk);
                RevertViableChunks();
            }

            currentChunk = newChunk;

            //worldGenerator.ChangeTilesInChunk(currentChunk);
            HighlightViableChunks(currentChunk.chunkPosition);


            Debug.Log($"GameObject is now in chunk at {currentChunk.chunkPosition}");
        }
    }

    void HighlightViableChunks(Vector3Int centerChunkPosition)
    {
        for (int y = -maxRadius; y <= maxRadius; y++)
        {
            for (int x = -maxRadius; x <= maxRadius; x++)
            {
                Vector3Int chunkPosition = new Vector3Int(centerChunkPosition.x + x, centerChunkPosition.y + y, 0);
                float distance = Mathf.Sqrt(x * x + y * y);

                if (distance >= minRadius && distance <= maxRadius)
                {
                    if (worldGenerator.generatedChunks.TryGetValue(chunkPosition, out ChunkData chunkData))
                    {
                        viableChunks.Add(chunkPosition);
                        //worldGenerator.ChangeTilesInChunk(chunkData);
                    }
                }
            }
        }
    }

    void RevertViableChunks()
    {
        foreach (var chunkPosition in viableChunks)
        {
            if (worldGenerator.generatedChunks.TryGetValue(chunkPosition, out ChunkData chunkData))
            {
                //worldGenerator.RevertTilesInChunk(chunkData);
            }
        }
        viableChunks.Clear();
    }

    void UpdateCreatureChunks()
    {
        foreach (var creature in creatures)
        {
            if (creature != null)
            {
                Vector3 creaturePosition = creature.transform.position;
                ChunkData newChunk = worldGenerator.GetChunkData(creaturePosition);

                if (newChunk != null)
                {
                    CreatureVitals vitals = creature.GetComponent<CreatureVitals>();
                    int populationValue = vitals.creatureObject.populationValue;

                    if (creatureChunks.TryGetValue(creature, out ChunkData oldChunk))
                    {
                        if (oldChunk != newChunk)
                        {
                            oldChunk.chunkPopulation -= populationValue;
                            newChunk.chunkPopulation += populationValue;
                            creatureChunks[creature] = newChunk;
                        }
                    }
                    else
                    {
                        newChunk.chunkPopulation += populationValue;
                        creatureChunks[creature] = newChunk;
                    }
                }
            }

        }
    }

    public void AddCreature(GameObject creature)
    {
        creatures.Add(creature);
        globalMobCount++;
    }

    public void RemoveCreature(GameObject creature)
    {
        if (creatures.Remove(creature))
        {
            globalMobCount--;
            if (creatureChunks.TryGetValue(creature, out ChunkData chunkData))
            {
                CreatureVitals vitals = creature.GetComponent<CreatureVitals>();
                int populationValue = vitals.creatureObject.populationValue;
                chunkData.chunkPopulation -= populationValue;
                creatureChunks.Remove(creature);
            }
        }
    }

    CreatureObject PickRandomCreatureObject()
    {
        // Calculate total weight
        int totalWeight = 0;
        foreach (var creatureObject in creatureObjects)
        {
            totalWeight += creatureObject.spawnWeight;
        }

        // Pick a random value within the total weight
        int randomValue = Random.Range(0, totalWeight);

        // Determine which creatureObject corresponds to the random value
        foreach (var creatureObject in creatureObjects)
        {
            if (randomValue < creatureObject.spawnWeight)
            {
                return creatureObject;
            }
            randomValue -= creatureObject.spawnWeight;
        }

        return null; // This should never happen if weights are properly set
    }

    void mobSpawner()
    {
        List<Vector3Int> viableChunkList = new List<Vector3Int>(viableChunks);
        Vector3Int randomChunkPosition = viableChunkList[Random.Range(0, viableChunkList.Count)];

        if (worldGenerator.generatedChunks.TryGetValue(randomChunkPosition, out ChunkData randomChunk))
        {
            CreatureObject randomCreatureObject = PickRandomCreatureObject();

            int currentMobPopulation = randomChunk.chunkPopulation;
            if (currentMobPopulation < maxGlobalChunkPopulation)
            {
                List<Vector3Int> tilePositions = new List<Vector3Int>(randomChunk.tileDepths.Keys);
                Vector3Int randomTilePosition = tilePositions[Random.Range(0, tilePositions.Count)];

                // Validate the tile position
                if (randomChunk.tileDepths.ContainsKey(randomTilePosition))
                {
                    // Convert tile position to world position
                    Vector3 worldPosition = worldGenerator.seaTilemap.CellToWorld(randomTilePosition);

                    // Log the tile position and world position
                    Debug.Log($"Spawning creature at tile position: {randomTilePosition} in chunk: {randomChunkPosition} with world position: {worldPosition}");

                    int packSize = Random.Range(randomCreatureObject.minPackSpawn, randomCreatureObject.maxPackSpawn);

                    for (int i = 0; i < packSize; i++)
                    {
                        // Instantiate the creature at the world position and set its parent to the tilemap
                        GameObject newCreature = Instantiate(creaturePrefab, worldPosition, Quaternion.identity, worldGenerator.seaTilemap.transform);
                        newCreature.GetComponent<CreatureVitals>().creatureObject = randomCreatureObject;
                        newCreature.GetComponent<CreatureBehaviour>().creatureObject = randomCreatureObject;

                        AddCreature(newCreature);
                        randomChunk.chunkPopulation += randomCreatureObject.populationValue;
                    }

                }
                else
                {
                    Debug.LogWarning($"Random tile position {randomTilePosition} is not valid in chunk: {randomChunkPosition}");
                }
            }
        }
    }

    void HandleDespawning()
    {
        Vector3 centerPosition = trackedObject.transform.position;
        Vector3Int centerChunkPosition = worldGenerator.WorldToChunkPosition(centerPosition);

        // Create a temporary list to store creatures that need to be despawned
        List<GameObject> creaturesToDespawn = new List<GameObject>();

        // Identify creatures that need to be despawned
        foreach (var creature in creatures)
        {
            if (creature != null) // Check if the creature is not null
            {
                Vector3 creaturePosition = creature.transform.position;
                Vector3Int creatureChunkPosition = worldGenerator.WorldToChunkPosition(creaturePosition);
                float distance = Vector3Int.Distance(creatureChunkPosition, centerChunkPosition);

                if (distance > maxRadius)
                {
                    creaturesToDespawn.Add(creature);
                }
            }
        }

        // Remove and destroy the creatures
        foreach (var creature in creaturesToDespawn)
        {
            RemoveCreature(creature);
            Destroy(creature);
        }

        // Remove null references from the creatures list
        creatures.RemoveAll(c => c == null);
    }

}
