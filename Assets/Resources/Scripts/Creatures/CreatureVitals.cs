using System.Collections;
using UnityEngine;

public class CreatureVitals : MonoBehaviour
{
    public float health;
    public float armor;
    public CreatureObject creatureObject;

    private bool isDamaged;
    private float currentDamage;

    public int minGoldDrop = 5;
    public int maxGoldDrop = 10;

    public ItemManager itemManager;

    private Color originalColor;
    private CreatureBehaviour creatureBehaviour;
    private Procedural procedural;
    CircleCollider2D circleCollider;


    void Start()
    {
        isDamaged = false;
        currentDamage = 0f;



        creatureBehaviour = GetComponent<CreatureBehaviour>();
        procedural = GetComponent<Procedural>();
        circleCollider = GetComponent<CircleCollider2D>();
        //circleCollider.radius = procedural.sizes[0];
        circleCollider.radius = 1f;

        itemManager = GameObject.Find("ghost").GetComponent<ItemManager>();

    }

    public void ApplyImpact(float damageMagnitude)
    {
        if (isDamaged)
        {
            if (damageMagnitude > currentDamage)
            {
                currentDamage = damageMagnitude;
            }
        }
    }

    private IEnumerator DamageCoroutine()
    {
        isDamaged = true;

        // Apply the damage to health, considering armor if needed
        ApplyDamage(currentDamage);

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


        isDamaged = false;
        currentDamage = 0f;
    }

    private void ApplyDamage(float damage)
    {
        // Assuming armor reduces damage by a flat amount
        float actualDamage = Mathf.Max(0, damage - armor);
        health -= actualDamage;
        Debug.Log("damaged");
        Debug.Log(health);

        creatureBehaviour.hostility += 20;

        // Check if health drops below zero
        if (health <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Debug.Log("Creature is dead.");

        // Determine the gold drop amount
        int goldDrop = Random.Range(minGoldDrop, maxGoldDrop + 1);

        // Instantiate the item prefabs based on the gold drop
        for (int i = 0; i < goldDrop; i++)
        {
            itemManager.CreateItem("goldOne", transform.position);
        }

        // Destroy the creature game object
        Destroy(gameObject);
    }
}
