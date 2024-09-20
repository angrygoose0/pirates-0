using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosions : MonoBehaviour
{
    public int raycastCount = 36; // Number of raycasts


    public GameObject explosionPrefab;


    public float duration;

    // Public variables to set in the editor
    public float initialLightIntensity = 1.5f;
    public float finalLightIntensity = 0f;
    public Vector3 initialLightScale = new Vector3(2, 1, 1);
    public Vector3 finalLightScale = Vector3.zero;
    public Color initialLightColor = Color.yellow;
    public Color finalLightColor = Color.red;
    public bool enableFlicker = true;
    public float flickerIntensityRange = 0.5f;
    public float pulsingSpeed = 4f; // Speed of pulsing effects


    public void LinePull(Vector3 explosionPosition, Transform playerTransform, float startAngle = 0f, float endAngle = 360f, float range = 0.5f, float speed = 10f)
    {
        if (startAngle >= endAngle)
        {
            Debug.LogError("Start angle must be less than end angle");
            return;
        }

        float angleRange = endAngle - startAngle;
        float angleStep = angleRange / raycastCount;

        for (int i = 0; i < raycastCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector3 rayDirection = Quaternion.Euler(0, 0, angle) * Vector3.right;

            // Adjust for isometric perspective by scaling the y component
            rayDirection = new Vector3(rayDirection.x, rayDirection.y * 0.5f, rayDirection.z);

            StartCoroutine(CastLineRay(explosionPosition, playerTransform, rayDirection, range, speed));
        }
    }
    private IEnumerator CastLineRay(Vector3 startPosition, Transform playerTransform, Vector3 direction, float range, float speed)
    {
        float dissipationRate = 1 / range;
        float currentRayForce = 1f; // slowly goes down from 1 - 0
        Vector3 currentPosition = startPosition;

        while (currentRayForce > 0)
        {
            int layerMask = LayerMask.GetMask("Items");
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, speed * Time.deltaTime, layerMask);

            if (hit.collider != null)
            {
                GameObject hitObject = hit.collider.gameObject;
                float distance = Vector2.Distance(currentPosition, hit.point);
                currentRayForce = Mathf.Max(currentRayForce - distance * dissipationRate, 0);

                if (currentRayForce > 0)
                {
                    Rigidbody2D rigidBody = hit.collider.GetComponent<Rigidbody2D>();

                    if (hitObject != null && hitObject.tag == "Item")
                    {
                        Debug.Log("Hit item!");

                        ItemScript itemScript = hitObject.GetComponent<ItemScript>();
                        if (!itemScript.beingReeled)
                        {
                            if (itemScript.isActive == false || itemScript.itemPickupable)
                            {
                                itemScript.beingReeled = true;
                                StartCoroutine(PullToPlayer(playerTransform, hitObject, itemScript));
                            }
                        }
                    }
                }
            }

            currentPosition += direction * speed * Time.deltaTime;
            currentRayForce -= dissipationRate * speed * Time.deltaTime;  // Removed redundant dissipation

            // Draw debug ray at the current position
            Debug.DrawRay(currentPosition, direction * speed * Time.deltaTime, Color.red, 0.1f);
            Debug.Log($"Current Ray Force: {currentRayForce} at position {currentPosition}");

            yield return null;
        }

    }


    private IEnumerator PullToPlayer(Transform playerTransform, GameObject itemObject, ItemScript itemScript)
    {
        float speed = 0f; // Start speed, adjust as necessary
        float maxSpeed = 50f; // Maximum speed it can accelerate to
        float acceleration = 50f; // How quickly the item will speed up

        while (Vector3.Distance(itemObject.transform.position, playerTransform.position) > 0.1f)
        {
            // Calculate the distance between the item and the player
            float distance = Vector3.Distance(itemObject.transform.position, playerTransform.position);

            // Gradually increase speed, with a cap on maximum speed
            speed = Mathf.Min(speed + acceleration * Time.deltaTime, maxSpeed);

            // Calculate a time-based interpolation factor for smooth movement
            float step = speed * Time.deltaTime / distance; // Normalized movement factor

            // Lerp the item's position towards the player's position using the step
            itemObject.transform.position = Vector3.Lerp(itemObject.transform.position, playerTransform.position, step);

            // Wait for the next frame

            yield return null;
        }

        itemScript.beingReeled = false;
    }




    public void Explode(Vector3 explosionPosition, ProjectileData projectile, float startAngle = 0f, float endAngle = 360f)
    {
        SingletonManager.Instance.feedbackManager.ExplosionFeedback(explosionPosition, projectile.explosionMultiplier);
        // Validate angles to ensure startAngle is less than endAngle
        if (startAngle >= endAngle)
        {
            Debug.LogError("Start angle must be less than end angle");
            return;
        }

        float angleRange = endAngle - startAngle;
        float angleStep = angleRange / raycastCount;
        GameObject explosionInstance = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);

        // Get the Light2D component from the explosion instance
        UnityEngine.Rendering.Universal.Light2D explosionLight = explosionInstance.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        if (explosionLight == null)
        {
            Debug.LogError("Explosion prefab does not have a Light2D component.");
            return;
        }

        // Set the initial properties of the light
        explosionLight.intensity = initialLightIntensity;
        explosionLight.transform.localScale = initialLightScale;
        explosionLight.color = initialLightColor;

        // Start the coroutine to handle the light's behavior
        StartCoroutine(HandleExplosionLight(explosionLight, duration));

        for (int i = 0; i < raycastCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector3 rayDirection = Quaternion.Euler(0, 0, angle) * Vector3.right;

            // Adjust for isometric perspective by scaling the y component
            rayDirection = new Vector3(rayDirection.x, rayDirection.y * 0.5f, rayDirection.z);

            StartCoroutine(CastRayUntilDissipated(explosionPosition, rayDirection, projectile));
        }
    }

    private IEnumerator HandleExplosionLight(UnityEngine.Rendering.Universal.Light2D explosionLight, float duration)
    {
        float elapsed = 0f;
        float initialIntensity = explosionLight.intensity;
        Vector3 initialScale = explosionLight.transform.localScale;
        Color initialColor = explosionLight.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Adjust the intensity, scale, and color over time
            explosionLight.intensity = Mathf.Lerp(initialIntensity, finalLightIntensity, t) * (1 + Mathf.Sin(t * Mathf.PI * pulsingSpeed));
            explosionLight.transform.localScale = Vector3.Lerp(initialScale, finalLightScale, t);
            explosionLight.color = Color.Lerp(initialColor, finalLightColor, t);

            // Apply flicker effect if enabled
            if (enableFlicker)
            {
                explosionLight.intensity += Random.Range(-flickerIntensityRange, flickerIntensityRange);
            }

            yield return null;
        }

        // Destroy the explosion instance after the duration
        Destroy(explosionLight.gameObject);
    }

    private IEnumerator CastRayUntilDissipated(Vector3 startPosition, Vector3 direction, ProjectileData projectile)
    {
        float dissipationRate = 1 / projectile.explosionRange;
        float currentRayForce = projectile.explosionInverse ? 0f : 1f; // Start the ray force based on the projectile.explosionInverse parameter
        Vector3 currentPosition = startPosition;

        while (projectile.explosionInverse ? currentRayForce < 1 : currentRayForce > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, projectile.explosionSpeed * Time.deltaTime);

            if (hit.collider != null)
            {
                GameObject hitObject = hit.collider.gameObject;
                float distance = Vector2.Distance(currentPosition, hit.point);
                currentRayForce = projectile.explosionInverse
                    ? Mathf.Min(currentRayForce + distance * dissipationRate, 1)
                    : Mathf.Max(currentRayForce - distance * dissipationRate, 0);

                if (projectile.explosionInverse ? currentRayForce < 1 : currentRayForce > 0)
                {
                    Rigidbody2D rigidBody = hit.collider.GetComponent<Rigidbody2D>();

                    Vector2 forceDirection = ((Vector2)hit.point - (Vector2)startPosition).normalized;
                    float appliedForce = currentRayForce * projectile.explosionMultiplier;
                    float appliedDamage = currentRayForce * projectile.damageMultiplier;

                    if (rigidBody != null)
                    {
                        rigidBody.AddForce(forceDirection * appliedForce, ForceMode2D.Impulse);
                    }

                    if (hitObject != null)
                    {
                        if (hitObject.tag == "Creature")
                        {
                            SingletonManager.Instance.creatureManager.ApplyImpact(hitObject, appliedDamage);  //change TODO
                        }

                    }
                    else
                    {
                        Debug.LogError("Hit object is null.");
                    }
                }
            }
            currentPosition += direction * projectile.explosionSpeed * Time.deltaTime;
            currentRayForce = projectile.explosionInverse
                ? Mathf.Min(currentRayForce + dissipationRate * projectile.explosionSpeed * Time.deltaTime, 1)
                : currentRayForce - dissipationRate * projectile.explosionSpeed * Time.deltaTime;

            // Draw the ray in the scene view for visualization
            Debug.DrawRay(startPosition, direction * (projectile.explosionInverse ? currentRayForce : (1 - currentRayForce) / dissipationRate), Color.red, 0.1f);

            yield return null;
        }
    }
}
