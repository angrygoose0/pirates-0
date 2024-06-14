using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosions : MonoBehaviour
{
    public float explosionForce = 50f; // Initial explosion force
    public int raycastCount = 36; // Number of raycasts
    public float forceDissipationRate = 5f; // Rate at which force dissipates per unit distance
    public float raySpeed = 10f; // Speed at which rays progress

    public void Explode(Vector3 explosionPosition)
    {
        float angleStep = 360f / raycastCount;

        for (int i = 0; i < raycastCount; i++)
        {
            float angle = i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, 0, angle) * Vector3.right;

            // Adjust for isometric perspective by scaling the y component
            rayDirection = new Vector3(rayDirection.x, rayDirection.y * 0.5f, rayDirection.z);

            StartCoroutine(CastRayUntilDissipated(explosionPosition, rayDirection));
        }
    }

    private IEnumerator CastRayUntilDissipated(Vector3 startPosition, Vector3 direction)
    {
        float currentForce = explosionForce;
        Vector3 currentPosition = startPosition;

        while (currentForce > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, raySpeed * Time.deltaTime);

            if (hit.collider != null)
            {
                float distance = Vector2.Distance(currentPosition, hit.point);
                currentForce = Mathf.Max(currentForce - distance * forceDissipationRate, 0);

                if (currentForce > 0)
                {
                    Rigidbody2D rb = hit.collider.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 forceDirection = ((Vector2)hit.point - (Vector2)currentPosition).normalized;
                        rb.AddForce(forceDirection * currentForce, ForceMode2D.Impulse);
                    }

                    // Move to the next position and continue casting the ray
                    currentPosition = (Vector2)hit.point + (Vector2)(direction * 0.1f); // Move a bit forward to avoid re-hitting the same collider

                    // Draw the ray in the scene view for visualization
                    Debug.DrawRay(startPosition, direction * distance, Color.red, 0.1f);
                }
                else
                {
                    break;
                }
            }
            else
            {
                // No collider hit, just reduce the force based on the dissipation rate over distance
                currentPosition += direction * raySpeed * Time.deltaTime;
                currentForce -= forceDissipationRate * raySpeed * Time.deltaTime;

                // Draw the ray in the scene view for visualization
                Debug.DrawRay(startPosition, direction * (explosionForce - currentForce) / forceDissipationRate, Color.red, 0.1f);
            }

            yield return null;
        }
    }
}
