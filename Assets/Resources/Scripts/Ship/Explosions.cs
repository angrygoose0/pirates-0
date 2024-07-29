using System.Collections;
using UnityEngine;

public class Explosions : MonoBehaviour
{
    public int raycastCount = 36; // Number of raycasts
    public CreatureManager creatureManager;
    public GameObject explosionPrefab;

    public void Explode(Vector3 explosionPosition, ItemObject itemObject)
    {
        float angleStep = 360f / raycastCount;
        GameObject explosionInstance = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);

        Destroy(explosionInstance, explosionInstance.GetComponent<ParticleSystem>().main.duration * 3f);
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, 0, angle) * Vector3.right;

            // Adjust for isometric perspective by scaling the y component
            rayDirection = new Vector3(rayDirection.x, rayDirection.y * 0.5f, rayDirection.z);

            StartCoroutine(CastRayUntilDissipated(explosionPosition, rayDirection, itemObject));
        }
    }


    private IEnumerator CastRayUntilDissipated(Vector3 startPosition, Vector3 direction, ItemObject itemObject)
    {
        float dissipationRate = 1 / itemObject.explosionRange;
        float currentRayForce = itemObject.explosionInverse ? 0f : 1f; // Start the ray force based on the itemObject.explosionInverse parameter
        Vector3 currentPosition = startPosition;

        while (itemObject.explosionInverse ? currentRayForce < 1 : currentRayForce > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, itemObject.explosionSpeed * Time.deltaTime);

            if (hit.collider != null)
            {
                GameObject hitObject = hit.collider.gameObject;
                float distance = Vector2.Distance(currentPosition, hit.point);
                currentRayForce = itemObject.explosionInverse
                    ? Mathf.Min(currentRayForce + distance * dissipationRate, 1)
                    : Mathf.Max(currentRayForce - distance * dissipationRate, 0);

                if (itemObject.explosionInverse ? currentRayForce < 1 : currentRayForce > 0)
                {
                    Rigidbody2D rigidBody = hit.collider.GetComponent<Rigidbody2D>();

                    Vector2 forceDirection = ((Vector2)hit.point - (Vector2)startPosition).normalized;
                    float appliedForce = currentRayForce * itemObject.explosionMultiplier;
                    float appliedDamage = currentRayForce * itemObject.damageMultiplier;

                    if (rigidBody != null)
                    {
                        rigidBody.AddForce(forceDirection * appliedForce, ForceMode2D.Impulse);
                    }

                    if (hitObject != null)
                    {
                        creatureManager.ApplyImpact(hitObject, appliedDamage, itemObject.effects);

                    }
                    else
                    {
                        Debug.LogError("Hit object is null.");
                    }
                }

            }
            currentPosition += direction * itemObject.explosionSpeed * Time.deltaTime;
            currentRayForce = itemObject.explosionInverse
                ? Mathf.Min(currentRayForce + dissipationRate * itemObject.explosionSpeed * Time.deltaTime, 1)
                : currentRayForce - dissipationRate * itemObject.explosionSpeed * Time.deltaTime;

            // Draw the ray in the scene view for visualization
            Debug.DrawRay(startPosition, direction * (itemObject.explosionInverse ? currentRayForce : (1 - currentRayForce) / dissipationRate), Color.red, 0.1f);

            yield return null;
        }
    }


}
