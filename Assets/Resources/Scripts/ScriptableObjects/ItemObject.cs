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
        int ammoCount = 1)
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
    }
}


[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item")]
public class ItemObject : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public float mass = 1f;

    public List<ProjectileData> projectileData = null;
    public int goldAmount = 0;
    public ItemObject spawningItem;
    public float regenerateTime;
    public List<Recipe> recipes;


    public List<AbilityData> abilityList;
    public Active active;

    public void UseActive(Vector3 position, Vector2 blockDirection)
    {
        if (active != null)
        {
            active.Activate(this, position, blockDirection);
        }
    }


}


