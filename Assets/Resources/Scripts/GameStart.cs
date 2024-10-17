using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public bool gameStarted = false;
    public bool trailer;
    public GameObject startButton;
    public ProjectileData projectileData;
    public Transform trailerCameraAnchor;
    public int creatureSpawnRadius;
    public List<CreatureObject> creatureObjectList;
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
            StartCoroutine(PlayTrailerSequence());
        }
    }

    IEnumerator PlayTrailerSequence()
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

        for (int i = 0; i < 5; i++)
        {
            List<Vector3Int> perimeterChunkList = GetPerimeterChunks(SingletonManager.Instance.worldGenerator._previousCenterChunkPosition, creatureSpawnRadius);
            int randomChunkIndex = Random.Range(0, perimeterChunkList.Count);
            Vector3Int chunkToSpawnCreature = perimeterChunkList[randomChunkIndex];
            ChunkData chunkData;
            SingletonManager.Instance.worldGenerator.generatedChunks.TryGetValue(chunkToSpawnCreature, out chunkData);

            int randomCreatureIndex = Random.Range(0, creatureObjectList.Count);
            CreatureObject creatureToSpawn = creatureObjectList[randomCreatureIndex];

            CreatureData newCreatureData = SingletonManager.Instance.creatureManager.SpawnCreature(Vector3.zero, creatureToSpawn, chunkData);

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
