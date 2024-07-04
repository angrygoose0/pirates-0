using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum State
{
    Idle,
    Aggressive,
    // Add more states here as needed
}

public class TentacleSegment
{
    public GameObject gameObject;
    public CircleCollider2D collider;
    public SpriteRenderer renderer;
    public Vector3 direction;
}

public class TentacleData
{
    public Vector3 targetPosition { get; set; }
    public Vector3Int currentTilePosition { get; set; }
    public Vector3 velocity;
    public Dictionary<GameObject, TentacleSegment> segments = new Dictionary<GameObject, TentacleSegment>();
    public float setDistance = 1.0f;
    public float moveSpeed = 5.0f;
    public float pullStrength = 0.5f;
    public float wiggleFrequency = 2.0f;
    public float wiggleAmplitude = 0.5f;
    public bool endTarget = true;
}

public class CreatureData
{
    public CreatureObject creatureObject;
    public Vector3Int currentTilePosition { get; set; }
    public List<Vector3Int> surroundingTiles { get; set; }
    public Vector3 targetPosition { get; set; }
    public Dictionary<GameObject, TentacleData> tentacles = new Dictionary<GameObject, TentacleData>();
    public State currentState;
    public float hostility;
    public Vector3 velocity;
    public bool isMovementCoroutineRunning = false;
    public float movementDelay;
    public GameObject targetShipPart = GameObject.Find("ghost");

}

public class CreatureManager : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public GameObject trackedObject;
    public Dictionary<GameObject, CreatureData> creatures; // Dictionary of all creatures
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
    public GameObject tentaclePrefab;
    public GameObject tentacleSegmentprefab;

    public Tilemap worldTilemap;

    void Start()
    {
        creatures = new Dictionary<GameObject, CreatureData>();
        globalMobCount = creatures.Count; // Initialize the global mob count
        worldTilemap = GameObject.Find("world").GetComponent<Tilemap>();
        GameObject tentacleContainer = GameObject.Find("tentacleContainer");
    }

    void Update()
    {
        TrackObjectChunk();
        UpdateCreatureChunks();

        foreach (KeyValuePair<GameObject, CreatureData> creatureEntry in creatures)
        {
            GameObject creatureGameObject = creatureEntry.Key;
            CreatureData creatureData = creatureEntry.Value;

            Vector3Int newTilePosition = worldGenerator.seaTilemap.WorldToCell(creatureGameObject.transform.position);
            if (creatureData.currentTilePosition != newTilePosition)
            {
                creatureData.currentTilePosition = newTilePosition;
                creatureData.surroundingTiles = GetSurroundingTiles(newTilePosition, creatureData.creatureObject.range);
            }

            float hostility = creatureData.hostility;
            float aggressionThreshold = creatureData.creatureObject.aggressionThreshold;
            State currentState = creatureData.currentState;

            if (hostility >= aggressionThreshold && currentState != State.Aggressive)
            {
                Debug.Log("agressive");
                creatureData.currentState = State.Aggressive;
            }
            else if (hostility < aggressionThreshold && currentState != State.Idle)
            {
                creatureData.currentState = State.Idle;
            }
            CreatureObject creatureObject = creatureData.creatureObject;


            Vector3Int creatureTargetTilemapPosition = worldTilemap.WorldToCell(creatureData.targetPosition);

            int deltaCreatureTargetX = Mathf.Abs(creatureTargetTilemapPosition.x - creatureData.currentTilePosition.x);
            int deltaCreatureTargetY = Mathf.Abs(creatureTargetTilemapPosition.y - creatureData.currentTilePosition.y);

            if (deltaCreatureTargetX + deltaCreatureTargetY == 0)
            {
                if (currentState == State.Idle)
                {
                    Vector3Int targetTile = creatureData.surroundingTiles[Random.Range(0, creatureData.surroundingTiles.Count)];
                    creatureData.targetPosition = worldTilemap.GetCellCenterLocal(targetTile);
                }
                else if (currentState == State.Aggressive)
                {
                    Vector3 targetShipPartLocalPosition = worldTilemap.WorldToCell(creatureData.targetShipPart.transform.position);
                    Vector3Int targetTile = Vector3Int.FloorToInt(targetShipPartLocalPosition);

                    creatureData.targetPosition = worldTilemap.GetCellCenterLocal(targetTile);
                }

            }

            UpdateMovement(creatureData.targetPosition, creatureObject.acceleration, creatureObject.maxMoveSpeed, creatureObject.deceleration, creatureObject.rotationSpeed, 1f, ref creatureData.velocity, creatureGameObject.transform);


            foreach (KeyValuePair<GameObject, TentacleData> tentacleEntry in creatureData.tentacles)
            {
                GameObject tentacleGameObject = tentacleEntry.Key;
                TentacleData tentacleData = tentacleEntry.Value;

                Vector3Int newTentacleTilePosition = worldGenerator.seaTilemap.WorldToCell(tentacleGameObject.transform.position);
                if (tentacleData.currentTilePosition != newTentacleTilePosition)
                {
                    tentacleData.currentTilePosition = newTentacleTilePosition;
                }

                if (tentacleData.endTarget == true)
                {
                    int deltaCurrentX = Mathf.Abs(tentacleData.currentTilePosition.x - creatureData.currentTilePosition.x);
                    int deltaCurrentY = Mathf.Abs(tentacleData.currentTilePosition.y - creatureData.currentTilePosition.y);

                    Vector3Int tentacleTargetTilemapPosition = worldTilemap.WorldToCell(creatureData.targetPosition);

                    int deltaTargetX = Mathf.Abs(tentacleTargetTilemapPosition.x - creatureData.currentTilePosition.x);
                    int deltaTargetY = Mathf.Abs(tentacleTargetTilemapPosition.y - creatureData.currentTilePosition.y);

                    if (deltaCurrentX + deltaCurrentY > 2 && deltaTargetX + deltaTargetY > 2)
                    {
                        Vector3Int targetTile = creatureData.surroundingTiles[Random.Range(0, creatureData.surroundingTiles.Count)];
                        tentacleData.targetPosition = worldTilemap.GetCellCenterLocal(targetTile);
                    }
                    UpdateMovement(tentacleData.targetPosition, 3f, 5f, 2f, 200f, 1f, ref tentacleData.velocity, tentacleGameObject.transform);
                }
                else
                {
                    //tentacleGameObject.transform = creatureGameObject.transform;
                }


                Dictionary<GameObject, TentacleSegment> segments = tentacleData.segments;
                List<GameObject> segmentKeys = new List<GameObject>(segments.Keys);
                for (int i = 0; i < segmentKeys.Count; i++)
                {


                    GameObject segmentKey = segmentKeys[i];
                    TentacleSegment currentSegment = segments[segmentKey];
                    Vector3 desiredPosition;

                    GameObject nextSegmentKey = null;
                    GameObject previousSegmentKey = null;


                    if (i == 0)
                    {

                        nextSegmentKey = segmentKeys[i + 1];
                        TentacleSegment nextSegment = segments[nextSegmentKey];

                        desiredPosition = creatureGameObject.transform.position;
                    }

                    else if (i == segmentKeys.Count - 1 && tentacleData.endTarget == true)
                    {
                        desiredPosition = tentacleGameObject.transform.position;
                        previousSegmentKey = segmentKeys[i - 1];
                        TentacleSegment previousSegment = segments[previousSegmentKey];
                    }

                    else
                    {
                        previousSegmentKey = segmentKeys[i - 1];
                        TentacleSegment previousSegment = segments[previousSegmentKey];

                        nextSegmentKey = segmentKeys[i + 1];
                        TentacleSegment nextSegment = segments[nextSegmentKey];

                        Vector3 directionPrev = segmentKey.transform.position - previousSegmentKey.transform.position;
                        Vector3 directionNext = nextSegment != null ? nextSegmentKey.transform.position - segmentKey.transform.position : Vector3.zero;

                        if (tentacleData.endTarget == true)
                        {
                            desiredPosition = nextSegmentKey != null ? (previousSegmentKey.transform.position + nextSegmentKey.transform.position) / 2.0f : previousSegmentKey.transform.position;
                        }
                        else
                        {
                            desiredPosition = previousSegmentKey.transform.position + directionPrev.normalized * tentacleData.setDistance;
                        }
                        float offset = Mathf.Sin(Time.time * tentacleData.wiggleFrequency + i * 0.5f) * tentacleData.wiggleAmplitude;
                        Vector3 perpendicular = Vector3.Cross(directionPrev, Vector3.forward).normalized;
                        desiredPosition += perpendicular * offset;
                    }

                    Vector3 currentPosition = segmentKey.transform.position;
                    Vector3 direction = desiredPosition - currentPosition;

                    // Adjust direction to slow down movement in the y-axis
                    direction.y *= 0.5f;

                    Vector3 adjustedTargetPosition = currentPosition + direction;
                    segmentKey.transform.position = Vector3.Lerp(currentPosition, adjustedTargetPosition, tentacleData.moveSpeed * Time.deltaTime);
                    currentSegment.direction = (segmentKey.transform.position - currentPosition).normalized;

                    if (nextSegmentKey != null)
                    {
                        Vector3 pullDirectionNext = segmentKey.transform.position - nextSegmentKey.transform.position;
                        float distanceNext = pullDirectionNext.magnitude;

                        if (distanceNext > tentacleData.setDistance)
                        {
                            Vector3 pullForceNext = pullDirectionNext.normalized * (distanceNext - tentacleData.setDistance) * tentacleData.pullStrength;
                            segmentKey.transform.position -= pullForceNext * Time.deltaTime;
                        }
                    }

                    if (previousSegmentKey != null && tentacleData.endTarget == true)
                    {
                        Vector3 pullDirectionPrev = previousSegmentKey.transform.position - segmentKey.transform.position;
                        float distancePrev = pullDirectionPrev.magnitude;
                        if (distancePrev > tentacleData.setDistance)
                        {
                            Vector3 pullForcePrev = pullDirectionPrev.normalized * (distancePrev - tentacleData.setDistance) * tentacleData.pullStrength;
                            segmentKey.transform.position += pullForcePrev * Time.deltaTime;
                        }
                    }
                }
            }
        }




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
                RevertViableChunks();
            }

            currentChunk = newChunk;
            HighlightViableChunks(currentChunk.chunkPosition);
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
        foreach (var creature in creatures.Keys)
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

    private IEnumerator MovementCoroutine(CreatureData creatureData)
    {
        creatureData.isMovementCoroutineRunning = true;
        while (true)
        {
            List<Vector3Int> surroundingTiles = creatureData.surroundingTiles;

            Vector3Int targetTile;
            if (surroundingTiles.Count > 0)
            {
                switch (creatureData.currentState)
                {
                    case State.Idle:
                        // Pick a random tile.
                        targetTile = surroundingTiles[Random.Range(0, surroundingTiles.Count)];
                        creatureData.targetPosition = worldTilemap.GetCellCenterLocal(targetTile);
                        break;
                    case State.Aggressive:


                        Vector3 targetShipPartLocalPosition = worldTilemap.WorldToCell(creatureData.targetShipPart.transform.position);
                        targetTile = Vector3Int.FloorToInt(targetShipPartLocalPosition);

                        creatureData.targetPosition = worldTilemap.GetCellCenterLocal(targetTile);
                        break;
                }
            }


            // Wait for the delay before picking a new targetTile
            yield return new WaitForSeconds(creatureData.movementDelay);
        }
    }

    public List<Vector3Int> GetSurroundingTiles(Vector3Int centerTile, float range)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        int rangeInt = Mathf.CeilToInt(range);

        for (int x = -rangeInt; x <= rangeInt; x++)
        {
            for (int y = -rangeInt; y <= rangeInt; y++)
            {
                Vector3Int tile = new Vector3Int(centerTile.x + x, centerTile.y + y, centerTile.z);
                if (Vector3Int.Distance(centerTile, tile) <= range)
                {
                    tiles.Add(tile);
                }
            }
        }

        return tiles;
    }
    public void UpdateMovement(Vector3 targetPosition, float acceleration, float maxMoveSpeed, float deceleration, float rotationSpeed, float movementMultiplier, ref Vector3 velocity, Transform transform)
    {
        float currentAcceleration = acceleration * movementMultiplier;
        float currentMaxMoveSpeed = maxMoveSpeed * movementMultiplier;

        Vector3 direction = targetPosition - transform.localPosition;
        float distance = direction.magnitude;
        direction.Normalize();

        Vector3 targetVelocity = direction * currentMaxMoveSpeed;
        targetVelocity.y *= 0.5f;

        if (distance > 0.1f)
        {
            velocity = Vector3.MoveTowards(velocity, targetVelocity, currentAcceleration * Time.deltaTime);
        }
        else
        {
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        transform.localPosition += velocity * Time.deltaTime;

        if (velocity.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
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
        int totalWeight = 0;
        foreach (var creatureObject in creatureObjects)
        {
            totalWeight += creatureObject.spawnWeight;
        }

        int randomValue = Random.Range(0, totalWeight);

        foreach (var creatureObject in creatureObjects)
        {
            if (randomValue < creatureObject.spawnWeight)
            {
                return creatureObject;
            }
            randomValue -= creatureObject.spawnWeight;
        }

        return null;
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

                if (randomChunk.tileDepths.ContainsKey(randomTilePosition))
                {
                    Vector3 worldPosition = worldGenerator.seaTilemap.CellToWorld(randomTilePosition);

                    int packSize = Random.Range(randomCreatureObject.minPackSpawn, randomCreatureObject.maxPackSpawn);

                    for (int i = 0; i < packSize; i++)
                    {
                        GameObject newCreature = Instantiate(creaturePrefab, worldPosition, Quaternion.identity, worldGenerator.seaTilemap.transform);
                        newCreature.GetComponent<CreatureVitals>().creatureObject = randomCreatureObject;



                        Vector3Int currentTilePosition = worldGenerator.seaTilemap.WorldToCell(worldPosition);
                        creatures.Add(newCreature, new CreatureData
                        {
                            creatureObject = randomCreatureObject,
                            currentTilePosition = currentTilePosition,
                            surroundingTiles = GetSurroundingTiles(currentTilePosition, randomCreatureObject.range),
                            targetPosition = newCreature.transform.position,
                            hostility = 1000,
                        });




                        globalMobCount++;

                        randomChunk.chunkPopulation += randomCreatureObject.populationValue;

                        CreatureData creatureData = creatures[newCreature];


                        List<TentacleValue> tentacleList = randomCreatureObject.tentacleList;


                        foreach (TentacleValue tentacle in tentacleList)
                        {

                            GameObject newTentacle = Instantiate(tentaclePrefab, worldPosition, Quaternion.identity, worldGenerator.seaTilemap.transform);
                            TentacleData tentacleData = new TentacleData
                            {
                                targetPosition = newCreature.transform.position,
                                currentTilePosition = currentTilePosition,
                            };

                            List<float> segmentSizeList = tentacle.segmentSizes;
                            foreach (float segmentSize in segmentSizeList)
                            {
                                GameObject newTentacleSegment = Instantiate(tentacleSegmentprefab, worldPosition, Quaternion.identity);
                                //newTentacleSegment.transform.SetParent();

                                //CircleCollider2D collider = newTentacleSegment.GetComponent<CircleCollider2D>();
                                //SpriteRenderer renderer = newTentacleSegment.GetComponent<SpriteRenderer>();
                                //collider.radius = segmentSize;
                                float diameter = segmentSize * 2.0f;
                                newTentacleSegment.transform.localScale = new Vector3(diameter, diameter, 1);
                                TentacleSegment tentacleSegmentData = new TentacleSegment
                                {
                                    //collider = collider,
                                    //renderer = renderer,
                                };
                                tentacleData.segments.Add(newTentacleSegment, tentacleSegmentData);


                            }
                            creatureData.tentacles.Add(newTentacle, tentacleData);

                        }


                    }
                }
            }
        }
    }

    void HandleDespawning()
    {
        Vector3 centerPosition = trackedObject.transform.position;
        Vector3Int centerChunkPosition = worldGenerator.WorldToChunkPosition(centerPosition);

        List<GameObject> creaturesToDespawn = new List<GameObject>();

        foreach (var creature in creatures.Keys)
        {
            if (creature != null)
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

        foreach (var creature in creaturesToDespawn)
        {
            RemoveCreature(creature);
            Destroy(creature);
        }

        List<GameObject> keysToRemove = new List<GameObject>();
        foreach (var kvp in creatures)
        {
            if (kvp.Key == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            creatures.Remove(key);
        }
    }
}
