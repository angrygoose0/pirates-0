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
    public int ammoCount = 0; //if 0, then it's not a spawned ammo,and doesnt lerp towards center after spawning.
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


