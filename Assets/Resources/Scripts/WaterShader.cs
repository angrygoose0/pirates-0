using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaterData
{
    public float size;
    public GameObject rippleObject;
    public Vector3[] segmentPositions;
    public Vector3[] segmentVelocity;
    public GameObject creature;

    // Constructor to initialize the data
    public WaterData(float size, GameObject rippleObject, Vector3[] segmentPositions, Vector3[] segmentVelocity, GameObject creature)
    {
        this.size = size;
        this.rippleObject = rippleObject;
        this.segmentPositions = segmentPositions;
        this.segmentVelocity = segmentVelocity;
        this.creature = creature;

    }
}

public class WaterShader : MonoBehaviour
{
    public Dictionary<Transform, WaterData> waterDataDict = new Dictionary<Transform, WaterData>();  // Initialize the dictionary
    public GameObject ripplePrefab;
    public int rippleLength;
    public float smoothSpeed;

    // Method to add new WaterData to the dictionary
    public void AddToWaterDataDict(Transform transform, float size, GameObject creature)
    {
        // If the transform doesn't already exist in the dictionary
        if (waterDataDict.ContainsKey(transform))
        {
            return;
        }

        GameObject newRippleObject = Instantiate(ripplePrefab, transform.position, Quaternion.identity, transform);
        LineRenderer lineRenderer = newRippleObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = rippleLength;
        Vector3[] segmentPoses = new Vector3[rippleLength];
        Vector3[] segmentV = new Vector3[rippleLength];


        // Initialize each element in both arrays with the default value
        for (int i = 0; i < rippleLength; i++)
        {
            segmentPoses[i] = transform.position;
            segmentV[i] = transform.position;
        }

        WaterData waterData = new WaterData(size, newRippleObject, segmentPoses, segmentV, creature);
        waterDataDict.Add(transform, waterData);  // Add WaterData using Transform as the key
    }

    // Method to remove WaterData from the dictionary
    public void RemoveFromWaterDataDict(Transform targetTransform)
    {
        // Efficiently remove by key
        if (waterDataDict.ContainsKey(targetTransform))
        {
            WaterData waterData = waterDataDict[targetTransform];

            // If a rippleObject exists, destroy it
            if (waterData.rippleObject != null)
            {
                Destroy(waterData.rippleObject);
            }
            waterDataDict.Remove(targetTransform);  // Remove WaterData associated with the Transform
        }
    }

    void Update()
    {
        foreach (KeyValuePair<Transform, WaterData> entry in waterDataDict)
        {
            Transform transform = entry.Key;
            WaterData waterData = entry.Value;


            UpdateRipple(transform, waterData);  // Pass size if needed for customization
        }
    }

    // Method to update the ripple effect based on point movement
    public void UpdateRipple(Transform transform, WaterData waterData)
    {
        waterData.segmentPositions[0] = transform.position;

        for (int i = 1; i < waterData.segmentPositions.Length; i++)
        {
            waterData.segmentPositions[i] = Vector3.SmoothDamp(waterData.segmentPositions[i], waterData.segmentPositions[i - 1], ref waterData.segmentVelocity[i], smoothSpeed);

        }
        LineRenderer lineRenderer = waterData.rippleObject.GetComponent<LineRenderer>();
        lineRenderer.material.SetVector("_RingSpawnPosition", waterData.creature.transform.position);
        lineRenderer.SetPositions(waterData.segmentPositions);


        // Set up width curve based on segment sizes
        AnimationCurve widthCurve = new AnimationCurve();
        for (int i = 1; i < waterData.segmentPositions.Length; i++)
        {
            float t = (float)i / (waterData.segmentPositions.Length - 1); // Normalized position along the line

            float distance = Vector3.Distance(waterData.creature.transform.position, waterData.segmentPositions[i]);
            float width = distance * 0.8f; // diameter
            widthCurve.AddKey(t, width);
        }
        lineRenderer.widthCurve = widthCurve;

        // Set the width multiplier to smooth out the edges
        lineRenderer.widthMultiplier = 1.0f;
        lineRenderer.numCapVertices = 10;  // Add vertices to round the caps

    }
}
