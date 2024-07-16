using System.Collections;
using UnityEngine;

public class ShipVitals : MonoBehaviour
{
    public float shipHealth = 100f;  // Initialize shipHealth with a value, for example 100.
    private bool isDamaged = false;
    private float currentDamage = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyImpact(5f);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ApplyImpact(10f);
        }
        Debug.Log("Ship health: " + shipHealth);
    }

    public void ApplyImpact(float damageMagnitude)
    {
        if (isDamaged)
        {
            if (damageMagnitude > currentDamage)
            {
                float damage = damageMagnitude - currentDamage;
                shipHealth -= damage;
            }
        }
        else
        {
            currentDamage = damageMagnitude;
            StartCoroutine(DamageCoroutine());

            // Trigger screen shake effect
            if (ScreenShake.instance != null)
            {
                ScreenShake.instance.Shake(0.2f, damageMagnitude * 0.1f);
            }
        }
    }

    private IEnumerator DamageCoroutine()
    {
        isDamaged = true;

        // Apply damage
        shipHealth -= currentDamage;

        // Check if health is less than or equal to zero
        if (shipHealth <= 0)
        {
            Debug.Log("Game over");
            yield break;  // Stop coroutine execution if the ship is destroyed
        }

        // Wait for invincibility duration
        yield return new WaitForSeconds(1f);

        // Reset damage
        currentDamage = 0f;
        isDamaged = false;
    }
}
