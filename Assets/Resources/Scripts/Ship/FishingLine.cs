using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishingLine : MonoBehaviour
{
    public Tilemap tilemap;
    public float gravity = -9.81f; // Gravity affecting the line
    public GameObject linePrefab;
    public float firingForceMultiplier;


    public void FireLine(Transform playerTransform, Vector3Int endTileCoordinate, float heldTime)
    {
        float firingForce = heldTime * 20f + 10f;
        StartCoroutine(MoveLine(playerTransform, endTileCoordinate, firingForce));
    }

    private IEnumerator MoveLine(Transform playerTransform, Vector3Int endTile, float firingForce)
    {
        // Instantiate the line at the playerTransform.position position
        GameObject line = Instantiate(linePrefab, playerTransform.position, Quaternion.identity);

        // Get the LineRenderer component
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer not found on linePrefab.");
            yield break;
        }

        // Initialize LineRenderer
        List<Vector3> linePoints = new List<Vector3> { playerTransform.position };

        Rigidbody2D rb = line.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Handle gravity manually

        float initialDistance = Vector3.Distance(playerTransform.position, tilemap.GetCellCenterWorld(endTile));
        float timeToTarget = Mathf.Sqrt(2 * initialDistance / (firingForce));

        // Calculate initial velocity components
        Vector3 initialEnd = tilemap.GetCellCenterWorld(endTile);
        Vector3 displacement = initialEnd - playerTransform.position;

        float initialVelocityX = displacement.x / timeToTarget;
        float initialVelocityY = (displacement.y - 0.5f * gravity * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

        rb.velocity = new Vector2(initialVelocityX, initialVelocityY);

        Vector3 groundEnd = tilemap.GetCellCenterWorld(endTile);

        float elapsedTime = 0f;

        // Simulate the line flight to the end position
        while (elapsedTime < timeToTarget)
        {
            elapsedTime += Time.deltaTime;

            // Update target position based on tilemap's current position
            Vector3 currentEnd = tilemap.GetCellCenterWorld(endTile);
            displacement = currentEnd - line.transform.position;

            float remainingTime = timeToTarget - elapsedTime;

            if (remainingTime > 0)
            {
                rb.velocity = new Vector2(
                    displacement.x / remainingTime,
                    (displacement.y - 0.5f * gravity * Mathf.Pow(remainingTime, 2)) / remainingTime
                );
            }

            // Apply gravity manually
            rb.velocity += new Vector2(0, gravity) * Time.deltaTime;

            // Add the current position of the line to the line points
            linePoints.Add(line.transform.position);
            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.SetPositions(linePoints.ToArray());

            // Simulate sagging effect
            for (int i = 0; i < linePoints.Count - 1; i++)
            {

                if (i == 0)
                {
                    linePoints[i] = playerTransform.position;
                    continue;

                }


                Vector3 currentPoint = linePoints[i];
                float t = (float)i / (linePoints.Count - 1);
                Vector3 groundPosition = Vector3.Lerp(playerTransform.position, groundEnd, t);

                currentPoint.x = Mathf.Lerp(currentPoint.x, groundPosition.x, -gravity * Time.deltaTime);
                currentPoint.y = Mathf.Lerp(currentPoint.y, groundPosition.y, -gravity * Time.deltaTime);


                if (currentPoint.y <= groundPosition.y)
                    currentPoint.y = groundPosition.y;

                linePoints[i] = currentPoint;

            }

            lineRenderer.SetPositions(linePoints.ToArray());

            yield return null;
        }

        elapsedTime = 0f;
        float totalReelTime = timeToTarget; // Reel back time could be the same as firing time

        while (true)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the proportion of the reeling back process
            float reelProgress = elapsedTime / totalReelTime;

            // Move each point back towards the startPosition
            for (int i = 1; i < linePoints.Count; i++)
            {
                // Lerp the points towards the player's position
                linePoints[i] = Vector3.Lerp(linePoints[i], playerTransform.position, reelProgress);
            }

            // Update the LineRenderer's positions to reflect the reeling back motion
            lineRenderer.SetPositions(linePoints.ToArray());

            // Check if the last point has been sufficiently reeled in
            if (Vector3.Distance(linePoints[linePoints.Count - 1], playerTransform.position) < 0.3f)
            {
                Destroy(line);
                ProjectileData lineProjectile = new ProjectileData();
                HookItemsBack(playerTransform, tilemap.GetCellCenterWorld(endTile));
                break;
            }

            yield return null;
        }

    }


    public int segmentCount;
    public float ovalWidth;
    public float ovalHeight;
    public GameObject hookPrefab;
    public void GenerateOvalCollider(PolygonCollider2D polyCollider)
    {
        Vector2[] points = new Vector2[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = (float)i / segmentCount * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * ovalWidth / 2;
            float y = Mathf.Sin(angle) * ovalHeight / 2;
            points[i] = new Vector2(x, y);
        }

        polyCollider.points = points;
    }

    public void HookItemsBack(Transform playerTransform, Vector3 endPosition)
    {
        GameObject hookObject = Instantiate(hookPrefab, endPosition, Quaternion.identity, tilemap.transform);
        GenerateOvalCollider(hookObject.GetComponent<PolygonCollider2D>());

        FishingHook hookScript = hookObject.GetComponent<FishingHook>();
        hookScript.playerTransform = playerTransform;

        Destroy(hookObject, 0.1f);

    }

}
