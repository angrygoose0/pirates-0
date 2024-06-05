using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "ScriptableObjects/Block", order = 1)]
public class BlockObject : ScriptableObject
{
    public float id;
    public Sprite blockSprite;
    public Sprite selectedSprite;
    public bool isCannon;

}
