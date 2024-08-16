using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosions : MonoBehaviour
{
    public int raycastCount = 36; // Number of raycasts
    public CreatureManager creatureManager;
    public GameObject explosionPrefab;
    public FeedbackManager feedbackManager;

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

    public void Explode(Vector3 explosionPosition, ProjectileData projectile, float startAngle, float endAngle, List<EffectData> effects)
    {
        feedbackManager.ExplosionFeedback(explosionPosition, projectile.explosionMultiplier);
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

            StartCoroutine(CastRayUntilDissipated(explosionPosition, rayDirection, projectile, effects));
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

    private IEnumerator CastRayUntilDissipated(Vector3 startPosition, Vector3 direction, ProjectileData projectile, List<EffectData> effects)
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
                            creatureManager.ApplyImpact(hitObject, appliedDamage, effects);  //change TODO
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
