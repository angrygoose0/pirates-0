using System.Collections;
using UnityEngine;

public class ShipVitals : MonoBehaviour
{
    public float shipHealth = 100f;  // Initialize shipHealth with a value, for example 100.
    private bool isDamaged = false;
    private float currentDamage = 0f;
    public DamageFlash damageFlash;
    public GameObject shipObject;

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
        //Debug.Log("Ship health: " + shipHealth);
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
        }
    }

    private IEnumerator DamageCoroutine()
    {
        isDamaged = true;

        // Apply damage
        shipHealth -= currentDamage;

        damageFlash.Flash(shipObject);

        // Check if health is less than or equal to zero
        if (shipHealth <= 0)
        {
            Debug.Log("Game over");
            yield break;  // Stop coroutine execution if the ship is destroyed
        }

        // Wait for invincibility duration
        yield return new WaitForSeconds(0.1f);

        // Reset damage
        currentDamage = 0f;
        isDamaged = false;
    }
}
