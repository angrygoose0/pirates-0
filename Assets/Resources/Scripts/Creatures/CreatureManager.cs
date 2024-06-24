using UnityEngine;
using System.Collections.Generic;

public class CreatureManager : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public GameObject trackedObject;
    public int minRadius = 5;
    public int maxRadius = 10;
    private ChunkData currentChunk;
    private HashSet<Vector3Int> viableChunks = new HashSet<Vector3Int>();

    void Update()
    {
        TrackObjectChunk();
    }

    void TrackObjectChunk()
    {
        Vector3 objectPosition = trackedObject.transform.position;
        ChunkData newChunk = worldGenerator.GetChunkData(objectPosition);

        if (newChunk != null && newChunk != currentChunk)
        {
            if (currentChunk != null)
            {
                worldGenerator.RevertTilesInChunk(currentChunk);
                RevertViableChunks();
            }

            currentChunk = newChunk;
            worldGenerator.ChangeTilesInChunk(currentChunk);
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
                        worldGenerator.ChangeTilesInChunk(chunkData);
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
                worldGenerator.RevertTilesInChunk(chunkData);
            }
        }
        viableChunks.Clear();
    }
}
