using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class ShipVitals : MonoBehaviour
{
    public float shipHealth;  // Initialize shipHealth with a value, for example 100.
    public float maxShipHealth = 100f;
    public FeedbackManager feedbackManager;

    public TextMeshProUGUI healthUI; // Reference to the UI Text component that displays the timer
    private bool isDamaged = false;
    private float currentDamage = 0f;
    public DamageFlash damageFlash;
    public GameObject shipObject;
    public AbilityManager abilityManager;
    public bool invincible = false;

    private TilemapRenderer tilemapRenderer;

    private float healthRegenRate = 2f; // Health points regenerated per second
    private float timeSinceLastDamage = 0f; // Time since the last damage was taken
    private float regenDelay = 5f; // Time in seconds before health starts regenerating


    void Start()
    {



        shipHealth = maxShipHealth;
        tilemapRenderer = shipObject.GetComponent<TilemapRenderer>();

        tilemapRenderer.material.SetFloat("_StretchInX", 1f);

        // Start the health regeneration coroutine
        StartCoroutine(HealthRegenCoroutine());
    }

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

        healthUI.text = shipHealth.ToString("F2");

        // Increment the timer
        if (!isDamaged)
        {
            timeSinceLastDamage += Time.deltaTime;
        }

        AbilityData health = abilityManager.GetAbilityData(Ability.Health);

        if (health != null)
        {
            maxShipHealth += health.tier * abilityManager.healthValue;
        }
    }

    public void ApplyImpact(float damageMagnitude)
    {
        feedbackManager.ShipDamagedFeedback(damageMagnitude);
        AbilityData fragility = abilityManager.GetAbilityData(Ability.Fragility);
        if (fragility != null)
        {
            damageMagnitude = damageMagnitude * abilityManager.fragilityValue * fragility.tier;
        }

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
        timeSinceLastDamage = 0f; // Reset the timer

        // Apply damage
        shipHealth -= currentDamage;

        tilemapRenderer.material.SetFloat("_WhiteAmount", 1f);

        // Check if health is less than or equal to zero
        if (shipHealth <= 0)
        {
            Debug.Log("Game over");
            yield break;  // Stop coroutine execution if the ship is destroyed
        }

        // Wait for invincibility duration
        yield return new WaitForSeconds(0.1f);

        tilemapRenderer.material.SetFloat("_WhiteAmount", 0f);

        // Reset damage
        currentDamage = 0f;
        isDamaged = false;
    }

    private IEnumerator HealthRegenCoroutine()
    {
        while (true)
        {
            // Check if the ship can regenerate health
            if (shipHealth < maxShipHealth && timeSinceLastDamage >= regenDelay)
            {
                // Regenerate health over time
                shipHealth += healthRegenRate * Time.deltaTime;
                shipHealth = Mathf.Min(shipHealth, maxShipHealth); // Ensure health does not exceed 100
            }

            yield return null; // Wait for the next frame
        }
    }
}
