using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item")]
public class ItemObject : ScriptableObject
{
    public string itemName;
    public float mass;
    public float damageMultiplier;
    public float explosionMultiplier;
    public float explosionRange;

}


