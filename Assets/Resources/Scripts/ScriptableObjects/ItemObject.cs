using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Recipe
{
    public List<ItemObject> materials;

}

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item")]
public class ItemObject : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public float mass;
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
    
}


