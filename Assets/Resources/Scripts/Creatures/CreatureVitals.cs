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

    // Update is called once per frame
    void Start()
    {
        isDamaged = false;
        currentDamage = 0f;
        currentForce = 0f;
    }

    public void TakeDamage(float damage)
    {
        if (isDamaged)
        {
            if (damage > currentDamage)
            {
                currentDamage = damage;
                StopCoroutine(DamageCoroutine());
                StartCoroutine(DamageCoroutine());
            }
        }
        else
        {
            currentDamage = damage;
            StartCoroutine(DamageCoroutine());
        }
    }

    public void ApplyForce(Vector2 force, float forceMagnitude)
    {
        if (isDamaged)
        {
            if (forceMagnitude > currentForce)
            {
                currentForce = forceMagnitude;
                StopCoroutine(DamageCoroutine());
                StartCoroutine(DamageCoroutine());
                GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
            }
        }
        else
        {
            currentForce = forceMagnitude;
            StartCoroutine(DamageCoroutine());
            GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        }
    }

    private IEnumerator DamageCoroutine()
    {
        isDamaged = true;
        // Apply the damage to health, considering armor if needed
        ApplyDamage(currentDamage);
        // Change color to red
        GetComponent<Renderer>().material.color = Color.red;

        yield return new WaitForSeconds(0.5f);

        // Revert the color back to the original (assuming white, change if necessary)
        GetComponent<Renderer>().material.color = Color.white;

        isDamaged = false;
        currentDamage = 0f;
        currentForce = 0f;
    }

    private void ApplyDamage(float damage)
    {
        // Assuming armor reduces damage by a flat amount
        float actualDamage = Mathf.Max(0, damage - armor);
        health -= actualDamage;

        // Check if health drops below zero
        if (health <= 0)
        {
            // Handle creature death (not implemented here)
            Debug.Log("Creature is dead.");
        }
    }
}
