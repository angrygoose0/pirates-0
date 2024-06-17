using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tentacle : MonoBehaviour
{
    public int length;
    public LineRenderer lineRend;
    public Vector3[] segmentPoses;
    private Vector3[] segmentV;

    public Transform targetDir;
    public float targetDist;
    public float smoothSpeed;

    public float wiggleSpeed;
    public float wiggleMagnitude;
    public Transform wiggleDir;

    public Transform creature; // Reference to the player's transform

    private Vector3 previousCreaturePosition;

    private void Start()
    {
        lineRend.positionCount = length;
        segmentPoses = new Vector3[length];
        segmentV = new Vector3[length];
        previousCreaturePosition = creature.position;
    }

    private void Update()
    {
        // Check if the creature has moved
        if (creature.position != previousCreaturePosition)
        {
            // Update targetDir to follow the player's position
            targetDir.position = creature.position;

            wiggleDir.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * wiggleSpeed) * wiggleMagnitude);

            segmentPoses[0] = targetDir.position;

            for (int i = 1; i < segmentPoses.Length; i++)
            {
                segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i - 1] + targetDir.right * targetDist, ref segmentV[i], smoothSpeed);
            }
            lineRend.SetPositions(segmentPoses);

            // Update the previous creature position
            previousCreaturePosition = creature.position;
        }
    }
}
