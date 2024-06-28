using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class StructureManager : MonoBehaviour
{
    public GameObject structurePrefab;
    public List<StructureObject> structureObjects; // List of StructureObjects


    public TileBase tile;
    public void GenerateStructure(Vector3 chunkCenterWorldPosition)
    {
        GameObject generatedStructure = Instantiate(structurePrefab, chunkCenterWorldPosition, Quaternion.identity);
        generatedStructure.transform.SetParent(GameObject.Find("world").transform);
        StructureObject structureObject = PickRandomStructureObject();

        Tilemap tilemap = generatedStructure.GetComponent<Tilemap>();

        // Calculate the start position for the rectangle, centering it around the pivot point
        int startX = -structureObject.width / 2;
        int startY = -structureObject.height / 2;

        // Loop through the width and height to set tiles in a rectangle pattern
        for (int x = 0; x < structureObject.width; x++)
        {
            for (int y = 0; y < structureObject.height; y++)
            {
                // Calculate the tile position in tilemap coordinates
                Vector3Int tilePosition = new Vector3Int(startX + x, startY + y, 1);

                // Set the tile at the calculated position
                tilemap.SetTile(tilePosition, tile);
            }
        }

    }

    StructureObject PickRandomStructureObject()
    {
        // Calculate total weight
        int totalWeight = 0;
        foreach (var structureObject in structureObjects)
        {
            totalWeight += structureObject.spawnWeight;
        }

        // Pick a random value within the total weight
        int randomValue = Random.Range(0, totalWeight);

        // Determine which structureObject corresponds to the random value
        foreach (var structureObject in structureObjects)
        {
            if (randomValue < structureObject.spawnWeight)
            {
                return structureObject;
            }
            randomValue -= structureObject.spawnWeight;
        }

        return null; // This should never happen if weights are properly set
    }

}
