using System.Collections;
using UnityEngine;

public class testExplosions : MonoBehaviour
{
    public int raycastCount = 36; // Number of raycasts
    public float raySpeed = 10f; // Speed at which rays progress
    public float explosionRange = 5f;
    public float explosionMultiplier = 5f;

    void Update()
    {
        // Check if the right mouse button is pressed
        if (Input.GetMouseButtonDown(1))
        {
            // Call the Explode method at position (0, 0)
            Explode(Vector3.zero);
        }
    }

    public void Explode(Vector3 explosionPosition)
    {
        float angleStep = 360f / raycastCount;

        for (int i = 0; i < raycastCount; i++)
        {
            float angle = i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, 0, angle) * Vector3.right;

            // Adjust for isometric perspective by scaling the y component
            rayDirection = new Vector3(rayDirection.x, rayDirection.y, rayDirection.z);

            StartCoroutine(CastRayUntilDissipated(explosionPosition, rayDirection));
        }
    }

    private IEnumerator CastRayUntilDissipated(Vector3 startPosition, Vector3 direction)
    {
        float dissipationRate = 1 / explosionRange;
        float currentRayForce = 1f; // Start the ray force at 1
        Vector3 currentPosition = startPosition;

        while (currentRayForce > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, raySpeed * Time.deltaTime);

            if (hit.collider != null)
            {
                float distance = Vector2.Distance(currentPosition, hit.point);
                currentRayForce = Mathf.Max(currentRayForce - distance * dissipationRate, 0);

                if (currentRayForce > 0)
                {
                    Rigidbody2D rigidBody = hit.collider.GetComponent<Rigidbody2D>();

                    Vector2 forceDirection = ((Vector2)hit.point - (Vector2)startPosition).normalized;
                    float appliedForce = currentRayForce * explosionMultiplier;

                    rigidBody.AddForce(forceDirection * appliedForce, ForceMode2D.Impulse);
                }

                // Draw the ray in the scene view for visualization
                Debug.DrawRay(startPosition, direction * distance, Color.red, 0.1f);

                // Break out of the loop to stop casting the ray
                break;
            }
            else
            {
                // No collider hit, just reduce the ray force based on the dissipation rate over distance
                currentPosition += direction * raySpeed * Time.deltaTime;
                currentRayForce -= dissipationRate * raySpeed * Time.deltaTime;

                // Draw the ray in the scene view for visualization
                Debug.DrawRay(startPosition, direction * (1 - currentRayForce) / dissipationRate, Color.red, 0.1f);
            }

            yield return null;
        }
    }
}
