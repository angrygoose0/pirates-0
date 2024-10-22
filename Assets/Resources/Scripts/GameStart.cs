using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public bool gameStarted = false;
    public int trailerType;
    public bool trailer;
    public GameObject startButton;
    public ProjectileData projectileData;
    public Transform trailerCameraAnchor;
    public int creatureSpawnRadius;
    public List<CreatureObject> creatureObjectList;
    // The decay factor (between 0 and 1), closer to 0 = slower decay
    float decayFactor = 0.9f;

    // How long the decay takes (in seconds)
    float decayDuration = 5f;
    void Awake()
    {
        gameStarted = false;
        startButton.SetActive(true);

    }

    public void StartGame()
    {
        gameStarted = true;
        startButton.SetActive(false);

        SingletonManager.Instance.cameraBrain.PlayCamera();

        if (!trailer)
        {
            SingletonManager.Instance.shipMovement.currentVelocity = new Vector2(0.5f, 0.5f);
        }

        if (trailer)
        {
            if (trailerType == 1)
            {
                StartCoroutine(PlayTrailer1Sequence());
            }
            else if (trailerType == 2)
            {
                StartCoroutine(PlayTrailer2Sequence());
            }

        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            SingletonManager.Instance.shipMovement.currentVelocity = new Vector2(0.5f, 0.5f);
        }

    }

    IEnumerator LightCoroutine()
    {
        float elapsedTime = 0f;
        float lightLevel = 1f;

        // Loop until the float value is close enough to 0
        while (lightLevel > 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float timeFactor = elapsedTime / decayDuration;
            lightLevel *= Mathf.Pow(decayFactor, Time.deltaTime);
            lightLevel = Mathf.Max(0f, lightLevel);

            SingletonManager.Instance.dayNightCycle.globalLight.intensity = lightLevel;
            SingletonManager.Instance.dayNightCycle.shipLight.intensity = 1 - lightLevel;

            yield return null;
        }
        lightLevel = 0f;
    }

    IEnumerator PlayTrailer2Sequence()
    {
        SingletonManager.Instance.dayNightCycle.globalLight.intensity = 1f;
        SingletonManager.Instance.dayNightCycle.shipLight.intensity = 0f;
        yield return new WaitForSeconds(2f);
        StartCoroutine(LightCoroutine());

        float spawnDelay = 2f;  // Start with 2 seconds delay

        // Loop to progressively spawn more creatures with reduced delays
        while (spawnDelay > 0.001f)
        {
            SingletonManager.Instance.creatureManager.mobSpawner(creatureObjectList[0]);
            yield return new WaitForSeconds(spawnDelay);
            spawnDelay *= 0.5f;
        }
        SingletonManager.Instance.creatureManager.mobSpawner(creatureObjectList[1]);

    }
    IEnumerator PlayTrailer1Sequence()
    {
        SingletonManager.Instance.cameraBrain.ChangeFollowTarget(0);
        yield return new WaitForSeconds(2f);
        CreatureData creatureData = SingletonManager.Instance.creatureManager.Trailer1();
        // First action: Wait for 2 seconds before doing something

        float accuracy = 1 / projectileData.accuracy;
        List<Vector3Int> tilesList = SingletonManager.Instance.interactionManager.GetSurroundingTiles(creatureData.currentTilePosition, accuracy);

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < 10; i++)
        {
            for (int o = 0; o < projectileData.fireAmount; o++)
            {
                Vector3Int targetTile = tilesList[Random.Range(0, tilesList.Count)];
                SingletonManager.Instance.cannonBehaviour.FireInTheHole(Vector3.zero, targetTile, projectileData, 2f);
            }

            // Wait for 0.01 seconds before the next iteration
            yield return new WaitForSeconds(0.1f);
        }
        // Second action: Wait for 5 seconds and then do something else
        yield return new WaitForSeconds(2f);

        SingletonManager.Instance.cameraBrain.Camera2();

        for (int i = 0; i < 500; i++)
        {
            List<Vector3Int> perimeterChunkList = GetPerimeterChunks(SingletonManager.Instance.worldGenerator._previousCenterChunkPosition, creatureSpawnRadius);
            int randomChunkIndex = Random.Range(0, perimeterChunkList.Count);
            Vector3Int chunkToSpawnCreature = perimeterChunkList[randomChunkIndex];
            ChunkData chunkData;
            SingletonManager.Instance.worldGenerator.generatedChunks.TryGetValue(chunkToSpawnCreature, out chunkData);

            int randomCreatureIndex = Random.Range(0, creatureObjectList.Count);
            CreatureObject creatureToSpawn = creatureObjectList[0];

            Vector3 worldPosition = SingletonManager.Instance.creatureManager.PickRandomTileFromChunk(chunkData);

            CreatureData newCreatureData = SingletonManager.Instance.creatureManager.SpawnCreature(worldPosition, creatureToSpawn, chunkData);

        }

    }



    public void EndGame()
    {
        gameStarted = false;
        startButton.SetActive(true);
    }

    public static List<Vector3Int> GetPerimeterChunks(Vector3Int center, int radius)
    {
        List<Vector3Int> perimeterPoints = new List<Vector3Int>();
        float epsilon = 0.1f;  // Small tolerance for floating-point comparisons

        // Loop through all possible points in the 2D square surrounding the circle
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int point = new Vector3Int(x, y, 0) + center;

                // Calculate the distance in 2D (ignoring Z)
                float distance = Mathf.Sqrt(x * x + y * y);

                // If the point is close to the perimeter (within a small range of radius)
                if (Mathf.Abs(distance - radius) <= epsilon)
                {
                    perimeterPoints.Add(point);
                }
            }
        }

        return perimeterPoints;
    }

}
