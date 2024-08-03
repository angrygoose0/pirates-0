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
