using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealisticWater : MonoBehaviour
{
    public GameObject waterPrefab;
    public Transform anchor;
    public int radius = 5;
    public int spriteSize = 512;

    private Dictionary<Vector2Int, GameObject> spawnedObjects = new Dictionary<Vector2Int, GameObject>();


}