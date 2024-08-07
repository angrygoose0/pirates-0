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

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item")]
public class ItemObject : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public float mass = 1f;
    public float reloadSpeed = 1f;
    public float damageMultiplier;
    public float explosionMultiplier;
    public float explosionRange;
    public float accuracy;
    public bool explosionInverse = false;
    public int fireAmount;
    public float explosionSpeed;
    public int goldAmount = 0;
    public ItemObject spawningItem;
    public float regenerateTime;
    public List<Recipe> recipes;
    public bool activeAbility;
    public List<EffectData> effects;
    public int ammoCount = 0; //if 0, then it's not a spawned ammo,and doesnt lerp towards center after spawning.
    public List<AbilityData> abilityList;

}


