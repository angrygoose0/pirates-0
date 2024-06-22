using System.Collections;
using UnityEngine;

public class CreatureVitals : MonoBehaviour
{
    public float health;
    public float armor;
    public CreatureObject creatureObject;

    private bool isDamaged;
    private float currentDamage;
    private float currentForce;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private CreatureBehaviour creatureBehaviour;

    void Start()
    {
        isDamaged = false;
        currentDamage = 0f;
        currentForce = 0f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer component is missing from this game object.");
        }

        creatureBehaviour = GetComponent<CreatureBehaviour>();
        if (creatureBehaviour == null)
        {
            Debug.LogError("CreatureBehaviour component not found on this GameObject.");
        }
    }

    public void ApplyImpact(Vector2 force, float forceMagnitude, float damageMagnitude)
    {
        if (isDamaged)
        {
            if (forceMagnitude > currentForce)
            {
                currentForce = forceMagnitude;
            }
            if (damageMagnitude > currentDamage)
            {
                currentDamage = damageMagnitude;
            }
        }
        else
        {
            currentDamage = damageMagnitude;
            currentForce = forceMagnitude;
            StartCoroutine(DamageCoroutine(force));
        }
    }

    private IEnumerator DamageCoroutine(Vector2 force)
    {
        isDamaged = true;

        // Apply the damage to health, considering armor if needed
        ApplyDamage(currentDamage);

        if (spriteRenderer != null)
        {
            // Change color to red
            spriteRenderer.color = Color.red;
        }

        // Apply knockback effect
        GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);

        // Jerk effect
        Vector3 originalPosition = transform.position;
        float jerkDuration = 0.1f;
        int jerkIterations = 10;
        float jerkAmount = 0.1f;

        for (int i = 0; i < jerkIterations; i++)
        {
            transform.position = originalPosition + (Vector3)Random.insideUnitCircle * jerkAmount;
            yield return new WaitForSeconds(jerkDuration / jerkIterations);
        }

        transform.position = originalPosition; // Ensure it returns to original position

        yield return new WaitForSeconds(0.4f);

        if (spriteRenderer != null)
        {
            // Revert the color back to the original
            spriteRenderer.color = originalColor;
        }

        isDamaged = false;
        currentDamage = 0f;
        currentForce = 0f;
    }

    private void ApplyDamage(float damage)
    {
        // Assuming armor reduces damage by a flat amount
        float actualDamage = Mathf.Max(0, damage - armor);
        health -= actualDamage;

        creatureBehaviour.hostility += 20;

        // Check if health drops below zero
        if (health <= 0)
        {
            // Handle creature death (not implemented here)
            Debug.Log("Creature is dead.");
        }
    }
}
