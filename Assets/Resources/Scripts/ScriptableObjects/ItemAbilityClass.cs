using UnityEngine;

public abstract class ItemAbilityClass : ScriptableObject
{
    public float cooldown;
    public abstract void Activate(ItemObject item, Vector3 position, Vector2 blockDirection);
}




