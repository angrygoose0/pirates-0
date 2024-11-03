using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Recipe
{
    public List<ItemObject> materials;

}

[System.Serializable]
public class LightEmission
{
    public float radius;

}

[System.Serializable]
public class ProjectileData
{
    public float reloadSpeed = 1f;
    public float damageMultiplier;
    public float explosionMultiplier;
    public float explosionRange;
    public float accuracy;
    public bool explosionInverse = false;
    public int fireAmount;
    public float explosionSpeed;
    public int ammoCount = 0;
    public Sprite sprite;
    public float cameraShake;

    // Constructor to initialize fields
    public ProjectileData(
        float reloadSpeed = 1f,
        float damageMultiplier = 1f,
        float explosionMultiplier = 1f,
        float explosionRange = 1f,
        float accuracy = 100f,
        bool explosionInverse = false,
        int fireAmount = 1,
        float explosionSpeed = 100f,
        int ammoCount = 1,
        Sprite sprite = null,
        float cameraShake = 1f
        )
    {
        this.reloadSpeed = reloadSpeed;
        this.damageMultiplier = damageMultiplier;
        this.explosionMultiplier = explosionMultiplier;
        this.explosionRange = explosionRange;
        this.accuracy = accuracy;
        this.explosionInverse = explosionInverse;
        this.fireAmount = fireAmount;
        this.explosionSpeed = explosionSpeed;
        this.ammoCount = ammoCount;
        this.sprite = sprite;
        this.cameraShake = cameraShake;
    }
}


[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item")]
public class ItemObject : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public float mass = 1f;
    public List<ProjectileData> projectile;
    public int goldAmount = 0;
    public ItemObject spawningItem;
    //public float regenerateTime;
    public List<Recipe> recipes;
    public bool affectsCannons = false;
    public Color glowColor;
    public int glowIntensity;
    public float pulseSpeed;
    // Set the range for intensity (0.2 == 20% down and 20% up from base)
    public float glowAmplitude = 0.2f;


    public List<AbilityData> abilityList;
    public ItemAbilityClass activeAbility;

    public void UseActive(Vector3 position, Direction blockDirection)
    {
        if (activeAbility != null)
        {
            Debug.Log(blockDirection);
            Vector2 directionVector = blockDirection.ToVector2().normalized;
            activeAbility.Activate(this, position, directionVector);
        }
    }


}


