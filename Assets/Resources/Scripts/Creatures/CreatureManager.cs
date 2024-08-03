using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;


[System.Serializable]
public enum State
{
    Idle,
    Aggressive,
    // Add more states here as needed
}

[System.Serializable]
public enum Effect
{
    Bleed,
    Slow,

}

[System.Serializable]
public class EffectData
{
    public Effect effect;
    public int tier;
    public float duration;

    // Constructor to initialize the EffectData
    public EffectData(Effect effect, int tier, float duration)
    {
        this.effect = effect;
        this.tier = tier;
        this.duration = duration;
    }
}

public class TentacleSegment
{
    public GameObject creature;
    public GameObject tentacle;
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
    public float maxMoveSpeed = 5.0f;
    public float deceleration;
    public float acceleration;
    public float pullStrength = 0.5f;
    public float wiggleFrequency = 2.0f;
    public float wiggleAmplitude = 0.5f;
    public bool endTarget = true;
    public LineRenderer lineRenderer;
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
    public float movementDelay;
    public GameObject targetShipPart = GameObject.Find("ghost");
    public float health;
    public bool isDamaged;
    public float currentDamage;
    public List<EffectData> effects = new List<EffectData>();
}

public class CreatureManager : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public GameObject trackedObject;
    public Dictionary<GameObject, CreatureData> creatures; // Dictionary of all creatures
    public List<CreatureObject> creatureObjects;
    public int minRadius = 5;
    public int maxRadius = 10;
    public Material damagedMaterial;
    public AbilityManager abilityManager;
    private ChunkData currentChunk;
    private HashSet<Vector3Int> viableChunks = new HashSet<Vector3Int>();
    private Dictionary<GameObject, ChunkData> creatureChunks = new Dictionary<GameObject, ChunkData>();
    private Dictionary<GameObject, GameObject> segmentToCreature = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, GameObject> segmentToTentacle = new Dictionary<GameObject, GameObject>();
    public int globalMobCount; // New global mob count
    public int maxGlobalMobCount = 70; // Maximum global mob count
    public int maxGlobalChunkPopulation = 50;
    public GameObject creaturePrefab;
    public GameObject tentacleSegmentprefab;
    public ItemManager itemManager;
    public Tilemap worldTilemap;
    public ShipVitals shipVitals;
    public Material creatureMaterial;
    public ItemObject goldItemObject;

    void Start()
    {
        creatures = new Dictionary<GameObject, CreatureData>();
        globalMobCount = creatures.Count; // Initialize the global mob count
        worldTilemap = GameObject.Find("world").GetComponent<Tilemap>();
        GameObject tentacleContainer = GameObject.Find("tentacleContainer");

        // Start the coroutine to update effects periodically
        StartCoroutine(CreatureTickRoutine());
    }


    void Update()
    {
        TrackObjectChunk();
        UpdateCreatureChunks();

        foreach (KeyValuePair<GameObject, CreatureData> creatureEntry in creatures)
        {
            GameObject creatureGameObject = creatureEntry.Key;
            CreatureData creatureData = creatureEntry.Value;

            Vector3Int newTilePosition = worldTilemap.WorldToCell(creatureGameObject.transform.position);
            if (creatureData.currentTilePosition != newTilePosition)
            {
                creatureData.currentTilePosition = newTilePosition;
                creatureData.surroundingTiles = GetSurroundingTiles(newTilePosition, creatureData.creatureObject.range);
            }

            float hostility = creatureData.hostility;
            float aggressionThreshold = creatureData.creatureObject.aggressionThreshold;
            State currentState = creatureData.currentState;
            bool stateChanged = false;
            if (hostility >= aggressionThreshold && currentState != State.Aggressive)
            {
                creatureData.currentState = State.Aggressive;
                stateChanged = true;

            }
            else if (hostility < aggressionThreshold && currentState != State.Idle)
            {
                creatureData.currentState = State.Idle;
                stateChanged = true;
            }
            //Debug.Log(creatureData.currentState);
            CreatureObject creatureObject = creatureData.creatureObject;


            Vector3Int creatureTargetTilemapPosition = worldTilemap.WorldToCell(creatureData.targetPosition);

            //Debug.Log(creatureTargetTilemapPosition + "target");
            //Debug.Log(creatureData.currentTilePosition + "current");
            //Debug.Log(worldTilemap.WorldToCell(creatureGameObject.transform.localPosition) + "gameobject");

            float movementMultiplier = 1f;


            var slowEffect = creatureData.effects
            .Where(effect => effect.effect == Effect.Slow)
            .OrderByDescending(effect => effect.tier)
            .FirstOrDefault();



            if (slowEffect != null)
            {
                int highestSlowTier = slowEffect.tier;
                float result = 1.0f - (highestSlowTier * 0.15f);
                movementMultiplier = Mathf.Max(result, 0); // Ensure the result is not negative
            }


            if (creatureTargetTilemapPosition == creatureData.currentTilePosition || stateChanged == true)
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

            creatureTargetTilemapPosition = worldTilemap.WorldToCell(creatureData.targetPosition);
            List<Vector3Int> creatureTargetSurroundingTiles = GetSurroundingTiles(creatureTargetTilemapPosition, creatureData.creatureObject.range);

            if (creatureData.isDamaged == false)
            {
                UpdateMovement(creatureData.targetPosition, creatureObject.acceleration, creatureObject.maxMoveSpeed, creatureObject.deceleration, creatureObject.rotationSpeed, movementMultiplier, ref creatureData.velocity, creatureGameObject.transform);
            }


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
                    int deltaCurrentY = Mathf.Abs(tentacleData.currentTilePosition.x - creatureData.currentTilePosition.y);

                    Vector3Int tentacleTargetTilemapPosition = worldTilemap.WorldToCell(tentacleData.targetPosition);

                    if (tentacleTargetTilemapPosition == tentacleData.currentTilePosition)
                    //
                    {
                        Vector3Int targetTile = creatureTargetSurroundingTiles[Random.Range(0, creatureTargetSurroundingTiles.Count)];
                        tentacleData.targetPosition = worldTilemap.GetCellCenterLocal(targetTile);
                    }
                    UpdateMovement(tentacleData.targetPosition, tentacleData.acceleration, tentacleData.maxMoveSpeed, tentacleData.deceleration, creatureObject.rotationSpeed, 1f, ref tentacleData.velocity, tentacleGameObject.transform);
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

                        if (i != segmentKeys.Count - 1)
                        {
                            nextSegmentKey = segmentKeys[i + 1];
                            TentacleSegment nextSegment = segments[nextSegmentKey];
                        }



                        Vector3 directionPrev = segmentKey.transform.position - previousSegmentKey.transform.position;
                        Vector3 directionNext = nextSegmentKey != null ? nextSegmentKey.transform.position - segmentKey.transform.position : Vector3.zero;

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
                    segmentKey.transform.position = Vector3.Lerp(currentPosition, adjustedTargetPosition, tentacleData.acceleration * Time.deltaTime * movementMultiplier);
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

                List<Vector3> splinePoints = GenerateCatmullRomSpline(segmentKeys);
                UpdateLineRenderer(tentacleData.lineRenderer, splinePoints);
            }
        }



        if (globalMobCount < maxGlobalMobCount)
        {
            mobSpawner();
        }
        HandleDespawning();
    }

    IEnumerator CreatureTickRoutine()
    {
        while (true)
        {
            UpdateEffects();
            UpdateHostility();
            yield return new WaitForSeconds(1f);
        }
    }

    void UpdateHostility()
    {
        foreach (var creaturePair in creatures)
        {
            CreatureData creatureData = creaturePair.Value;

            creatureData.hostility += 1f;
        }
    }

    void UpdateEffects()
    {
        List<EffectData> emptyEffectsList = new List<EffectData>();
        foreach (var creaturePair in creatures)
        {
            CreatureData creatureData = creaturePair.Value;

            // Create a dictionary to track the longest effect by type and tier
            Dictionary<(Effect, int), EffectData> effectDict = new Dictionary<(Effect, int), EffectData>();

            // Iterate through the effects list
            foreach (var effect in creatureData.effects)
            {
                var key = (effect.effect, effect.tier);

                // If the effect type and tier is not in the dictionary, add it
                // or if it has a longer duration than the existing one, update it
                if (!effectDict.ContainsKey(key) || effectDict[key].duration < effect.duration)
                {
                    effectDict[key] = effect;
                }
            }

            // Update the effects list to contain only the longest duration effects
            creatureData.effects = new List<EffectData>(effectDict.Values);

            // Check if the creature has the Bleed effect and get the strongest tier
            EffectData strongestBleedEffect = null;
            foreach (var effect in creatureData.effects)
            {
                if (effect.effect == Effect.Bleed)
                {
                    if (strongestBleedEffect == null || effect.tier > strongestBleedEffect.tier)
                    {
                        strongestBleedEffect = effect;
                    }
                }
            }


            // Get the first tentacle segment
            if (creatureData.tentacles.Count > 0 && strongestBleedEffect != null)
            {
                var firstTentacle = creatureData.tentacles.Values.FirstOrDefault();
                if (firstTentacle != null && firstTentacle.segments.Count > 0)
                {
                    var firstSegmentKey = firstTentacle.segments.Keys.FirstOrDefault();
                    if (firstSegmentKey != null)
                    {
                        float bleedDamageMagnitude = 1.5f * strongestBleedEffect.tier;
                        Debug.Log("bleed damage");
                        ApplyImpact(firstSegmentKey, bleedDamageMagnitude, emptyEffectsList);


                    }
                }
            }

            // Reduce the duration of each effect
            for (int i = creatureData.effects.Count - 1; i >= 0; i--)
            {
                creatureData.effects[i].duration -= 1f;

                // If the duration is zero or less, remove the effect
                if (creatureData.effects[i].duration <= 0)
                {
                    creatureData.effects.RemoveAt(i);
                }
            }
        }
    }


    void UpdateLineRenderer(LineRenderer lineRenderer, List<Vector3> splinePoints)
    {
        if (lineRenderer == null) return;
        List<Vector3> squashedPoints = new List<Vector3>();
        foreach (var point in splinePoints)
        {
            squashedPoints.Add(new Vector3(point.x * 0.5f, point.y, point.z));
        }
        lineRenderer.positionCount = squashedPoints.Count;
        lineRenderer.SetPositions(squashedPoints.ToArray());
    }


    List<Vector3> GenerateCatmullRomSpline(List<GameObject> segments, int resolution = 10)
    {
        List<Vector3> splinePoints = new List<Vector3>();
        for (int i = 0; i < segments.Count - 1; i++)
        {
            Vector3 p0 = segments[Mathf.Max(i - 1, 0)].transform.position;
            Vector3 p1 = segments[i].transform.position;
            Vector3 p2 = segments[i + 1].transform.position;
            Vector3 p3 = segments[Mathf.Min(i + 2, segments.Count - 1)].transform.position;

            for (int j = 0; j < resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 position = GetCatmullRomPosition(t, p0, p1, p2, p3);
                splinePoints.Add(position);
            }
        }
        return splinePoints;
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float a = -0.5f * t3 + t2 - 0.5f * t;
        float b = 1.5f * t3 - 2.5f * t2 + 1.0f;
        float c = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
        float d = 0.5f * t3 - 0.5f * t2;

        return a * p0 + b * p1 + c * p2 + d * p3;
    }



    public void ApplyImpact(GameObject hitSegmentObject, float damageMagnitude, List<EffectData> effectsList)
    {

        GameObject hitCreatureObject = segmentToCreature[hitSegmentObject];
        GameObject hitTentacleObject = segmentToTentacle[hitSegmentObject];
        CreatureData hitCreatureData = creatures[hitCreatureObject];


        hitCreatureObject.transform.position = hitSegmentObject.transform.position;

        AbilityData might = abilityManager.GetAbilityData(Ability.Might);
        if (might != null)
        {
            damageMagnitude = damageMagnitude * abilityManager.mightValue * might.tier;
        }

        TentacleData hitTentacleData = hitCreatureData.tentacles[hitTentacleObject];


        foreach (EffectData effectData in effectsList)
        {
            EffectData newEffect = new EffectData(effectData.effect, effectData.tier, effectData.duration);
            hitCreatureData.effects.Add(newEffect);
        }
        if (hitCreatureData.isDamaged)
        {
            if (damageMagnitude > hitCreatureData.currentDamage)
            {
                float damage = damageMagnitude - hitCreatureData.currentDamage;
                hitCreatureData.health -= damage;

            }
        }
        else
        {
            hitCreatureData.currentDamage = damageMagnitude;
            hitTentacleData.lineRenderer.material.SetFloat("_WhiteAmount", 1f);
            StartCoroutine(DamageCoroutine(hitCreatureObject, hitTentacleData.lineRenderer));
        }

        AbilityData lifeSteal = abilityManager.GetAbilityData(Ability.LifeSteal);

        if (lifeSteal != null)
        {
            shipVitals.shipHealth += damageMagnitude * abilityManager.lifeStealValue * lifeSteal.tier;

            // Check if shipHealth exceeds maxShipHealth
            if (shipVitals.shipHealth > shipVitals.maxShipHealth)
            {
                // Set shipHealth to maxShipHealth if it exceeds the maximum limit
                shipVitals.shipHealth = shipVitals.maxShipHealth;
            }
        }
        // need to move this to damagecoroutine, but it lags out the dmageflash
    }

    public void CreatureDeath(GameObject creatureObject)
    {
        CreatureData creatureData = creatures[creatureObject];
        foreach (KeyValuePair<GameObject, TentacleData> tentacleEntry in creatureData.tentacles)
        {
            TentacleData tentacleData = tentacleEntry.Value;
            foreach (KeyValuePair<GameObject, TentacleSegment> segmentEntry in tentacleData.segments)
            {
                GameObject segmentObject = segmentEntry.Key;


                if (segmentToCreature.TryGetValue(segmentObject, out GameObject creature))
                {
                    segmentToCreature.Remove(segmentObject);

                }
                if (segmentToTentacle.TryGetValue(segmentObject, out GameObject tentacle))
                {
                    segmentToTentacle.Remove(segmentObject);

                }

                Destroy(segmentObject);
            }
            Destroy(tentacleEntry.Key);
        }


        globalMobCount--;
        if (creatureChunks.TryGetValue(creatureObject, out ChunkData chunkData))
        {
            int populationValue = creatureData.creatureObject.populationValue;
            chunkData.chunkPopulation -= populationValue;
            creatureChunks.Remove(creatureObject);
        }

        creatures.Remove(creatureObject);
        Destroy(creatureObject);

    }
    private IEnumerator DamageCoroutine(GameObject creatureObject, LineRenderer lineRenderer)
    {
        CreatureData creatureData = creatures[creatureObject];
        creatureData.isDamaged = true;

        float damageDone = Mathf.Max(creatureData.currentDamage - creatureData.creatureObject.armor, 0);
        creatureData.health -= damageDone;



        if (creatureData.health <= 0)
        {
            Vector3 creaturePosition = creatureObject.transform.position;
            int goldDrop = Random.Range((int)creatureData.creatureObject.goldDropRange.x, (int)creatureData.creatureObject.goldDropRange.y + 1);




            // Instantiate the item prefabs based on the gold drop
            for (int i = 0; i < goldDrop; i++)
            {
                GameObject createdItem = itemManager.CreateItem(goldItemObject, creaturePosition);

            }
            ItemObject dropItemObject = creatureData.creatureObject.DetermineDrop();

            if (dropItemObject != null)
            {
                GameObject droppedItem = itemManager.CreateItem(dropItemObject, creaturePosition);
            }


            CreatureDeath(creatureObject);



            yield break;
        }

        List<GameObject> segmentKeys = new List<GameObject>();

        // Iterate through each TentacleData in the tentacles dictionary
        foreach (var tentacle in creatureData.tentacles.Values)
        {
            // Add each GameObject key from the segments dictionary to the list
            segmentKeys.AddRange(tentacle.segments.Keys);
        }



        Debug.Log("white");
        yield return new WaitForSeconds(0.1f);

        lineRenderer.material.SetFloat("_WhiteAmount", 0f);
        creatureData.currentDamage = 0f;
        creatureData.isDamaged = false;

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
        foreach (var kvp in creatures)
        {
            GameObject creature = kvp.Key;
            CreatureData creatureData = kvp.Value;
            if (creature != null)
            {
                Vector3 creaturePosition = creature.transform.position;
                ChunkData newChunk = worldGenerator.GetChunkData(creaturePosition);

                if (newChunk != null)
                {
                    int populationValue = creatureData.creatureObject.populationValue;

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

    public void AttackShip(GameObject creature)
    {
        CreatureData creatureData = creatures[creature];
        //creatureData.hostility = 0f;

        shipVitals.ApplyImpact(creatureData.creatureObject.damage);
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
        // Calculate effective acceleration and max speed with movement multiplier
        float currentAcceleration = acceleration * movementMultiplier;
        float currentMaxMoveSpeed = maxMoveSpeed * movementMultiplier;

        // Calculate direction to the target position
        Vector3 direction = targetPosition - transform.localPosition;
        // Adjust direction for isometric movement
        Vector3 adjustedDirection = new Vector3(direction.x, direction.y * 0.5f, direction.z);
        float distance = adjustedDirection.magnitude;
        adjustedDirection.Normalize();

        // Calculate the target velocity based on the direction and max speed
        Vector3 targetVelocity = adjustedDirection * currentMaxMoveSpeed;

        if (distance > 0.1f) // If not close enough to the target
        {
            // Move towards the target velocity with given acceleration
            velocity = Vector3.MoveTowards(velocity, targetVelocity, currentAcceleration * Time.deltaTime);
        }
        else // Close to the target, start deceleration
        {
            // Slow down the velocity to zero with deceleration
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Update position based on current velocity
        transform.localPosition += velocity * Time.deltaTime;

        // If the object is moving, update its rotation
        if (velocity.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }




    CreatureObject PickRandomCreatureObject(int hostilityLevel)
    {
        int totalWeight = 0;
        Dictionary<CreatureObject, int> adjustedWeights = new Dictionary<CreatureObject, int>();

        // Calculate the adjusted weights and total weight
        foreach (var creatureObject in creatureObjects)
        {
            int adjustedWeight = Mathf.RoundToInt(creatureObject.spawnWeight + (creatureObject.hostilityMultiplier * hostilityLevel));
            adjustedWeights[creatureObject] = adjustedWeight;
            totalWeight += adjustedWeight;
        }

        // Get a random value within the total weight
        int randomValue = Random.Range(0, totalWeight);

        // Select a creature based on the adjusted weights
        foreach (var creatureObject in creatureObjects)
        {
            if (randomValue < adjustedWeights[creatureObject])
            {
                return creatureObject;
            }
            randomValue -= adjustedWeights[creatureObject];
        }
        return null;
    }



    void mobSpawner()
    {
        List<Vector3Int> viableChunkList = new List<Vector3Int>(viableChunks);
        Vector3Int randomChunkPosition = viableChunkList[Random.Range(0, viableChunkList.Count)];
        ChunkData randomChunk;
        worldGenerator.generatedChunks.TryGetValue(randomChunkPosition, out randomChunk);

        if (randomChunk != null)
        {
            CreatureObject randomCreatureObject = PickRandomCreatureObject(randomChunk.chunkHostility);

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
                        CreatureBehaviour creatureBehaviour = newCreature.AddComponent<CreatureBehaviour>();
                        creatureBehaviour.creatureManager = this;



                        Vector3Int currentTilePosition = worldGenerator.seaTilemap.WorldToCell(worldPosition);
                        creatures.Add(newCreature, new CreatureData
                        {
                            creatureObject = randomCreatureObject,
                            currentTilePosition = currentTilePosition,
                            surroundingTiles = GetSurroundingTiles(currentTilePosition, randomCreatureObject.range),
                            targetPosition = newCreature.transform.position,
                            hostility = 0,
                            health = randomCreatureObject.startingHealth,
                        });


                        globalMobCount++;

                        randomChunk.chunkPopulation += randomCreatureObject.populationValue;

                        CreatureData creatureData = creatures[newCreature];


                        List<TentacleValue> tentacleList = randomCreatureObject.tentacleList;


                        foreach (TentacleValue tentacle in tentacleList)
                        {

                            GameObject newTentacle = Instantiate(creaturePrefab, worldPosition, Quaternion.identity, worldGenerator.seaTilemap.transform);
                            TentacleData tentacleData = new TentacleData
                            {
                                targetPosition = newCreature.transform.position,
                                currentTilePosition = currentTilePosition,
                                setDistance = tentacle.setDistance,
                                maxMoveSpeed = tentacle.maxMoveSpeed,
                                deceleration = tentacle.deceleration,
                                acceleration = tentacle.acceleration,
                                pullStrength = tentacle.pullStrength,
                                wiggleFrequency = tentacle.wiggleFrequency,
                                wiggleAmplitude = tentacle.wiggleAmplitude,
                                endTarget = tentacle.endTarget,
                            };
                            bool firstSegment = true;
                            List<float> segmentSizeList = tentacle.segmentSizes;
                            foreach (float segmentSize in segmentSizeList)
                            {
                                GameObject newTentacleSegment = Instantiate(tentacleSegmentprefab, worldPosition, Quaternion.identity, worldGenerator.seaTilemap.transform);
                                //newTentacleSegment.transform.SetParent();

                                CircleCollider2D collider = newTentacleSegment.GetComponent<CircleCollider2D>();
                                //SpriteRenderer renderer = newTentacleSegment.GetComponent<SpriteRenderer>();
                                collider.radius = segmentSize;
                                //float diameter = segmentSize * 2.0f;
                                //newTentacleSegment.transform.localScale = new Vector3(diameter, diameter, 1);
                                TentacleSegment tentacleSegmentData = new TentacleSegment
                                {
                                    creature = newCreature,
                                    tentacle = newTentacle,
                                    collider = collider,
                                    //renderer = renderer,
                                };
                                tentacleData.segments.Add(newTentacleSegment, tentacleSegmentData);

                                segmentToCreature[newTentacleSegment] = newCreature; // Add to reverse lookup
                                segmentToTentacle[newTentacleSegment] = newTentacle;

                                if (firstSegment)
                                {
                                    firstSegment = false;
                                    tentacleData.lineRenderer = newTentacleSegment.AddComponent<LineRenderer>();
                                    tentacleData.lineRenderer.material = creatureMaterial;
                                    tentacleData.lineRenderer.material.SetFloat("_StretchInX", 2f);
                                    tentacleData.lineRenderer.material.SetColor("_OriginalColor", Color.black);
                                    tentacleData.lineRenderer.sortingLayerName = "creature";

                                    AnimationCurve widthCurve = new AnimationCurve();
                                    for (int o = 0; o < segmentSizeList.Count; o++)
                                    {
                                        float t = (float)o / (segmentSizeList.Count - 1); // Normalized position along the line
                                        float width = segmentSizeList[o] * 2.0f; // Diameter
                                        widthCurve.AddKey(t, width);
                                    }
                                    tentacleData.lineRenderer.widthCurve = widthCurve;
                                    tentacleData.lineRenderer.widthMultiplier = 1.0f;
                                    tentacleData.lineRenderer.numCapVertices = 10;


                                }

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
            CreatureDeath(creature);
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
