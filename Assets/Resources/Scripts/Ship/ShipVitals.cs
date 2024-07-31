using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class ShipVitals : MonoBehaviour
{
    public float shipHealth = 100f;  // Initialize shipHealth with a value, for example 100.

    public TextMeshProUGUI healthUI; // Reference to the UI Text component that displays the timer
    private bool isDamaged = false;
    private float currentDamage = 0f;
    public DamageFlash damageFlash;
    public GameObject shipObject;

    private TilemapRenderer tilemapRenderer;

    void Start()
    {
        tilemapRenderer = shipObject.GetComponent<TilemapRenderer>();

        tilemapRenderer.material.SetFloat("_StretchInX", 1f);

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
}
